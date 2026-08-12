using System.Text.Json;
using Common.Application.Paging;
using Microsoft.Extensions.Logging;

namespace TrackHub.Manager.Application.GpsIntegration.Commands;

[Authorize(Resource = Resources.SynchronizedDevices, Action = Actions.Write, PrincipalTypes = "User,ServiceClient")]
[AllowCrossAccount("SyncWorker's device-sync loop enumerates every account via accountSettingsMaster and pushes the synchronized device catalog per account under one global router_client/syncworker_client identity.")]
public readonly record struct SynchronizeOperatorDevicesCommand(
    Guid AccountId,
    Guid OperatorId,
    IReadOnlyCollection<DeviceDto> Devices,
    string CorrelationId,
    string TriggerType = "AUTOMATIC",
    bool? AutoAssignNewDevices = null) : IRequest<OperatorSyncRunVm>;

public class SynchronizeOperatorDevicesCommandHandler(
    IDeviceWriter deviceWriter,
    IDeviceReader deviceReader,
    ITransporterWriter transporterWriter,
    ITransporterDeviceAssignmentWriter assignmentWriter,
    IGroupReader groupReader,
    IGroupWriter groupWriter,
    ITransporterGroupWriter transporterGroupWriter,
    IOperatorWriter operatorWriter,
    IAlertEventWriter alertWriter,
    ILogger<SynchronizeOperatorDevicesCommandHandler> logger)
    : IRequestHandler<SynchronizeOperatorDevicesCommand, OperatorSyncRunVm>
{
    public async Task<OperatorSyncRunVm> Handle(SynchronizeOperatorDevicesCommand request, CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        if (request.Devices.Any(d => d.AccountId != request.AccountId || d.OperatorId != request.OperatorId))
        {
            throw new ForbiddenAccessException();
        }

        var existing = await deviceReader.GetDevicesByOperatorAsync(request.OperatorId, cancellationToken);
        var existingByIdentifier = existing
            .GroupBy(d => d.Identifier)
            .ToDictionary(g => g.Key, g => g.First());

        var incomingIdentifiers = new HashSet<int>();
        var newlyAdded = new List<DeviceDto>();
        var newlyAddedDevices = new List<(DeviceDto Incoming, DeviceVm Device)>();
        int added = 0, updated = 0, ignored = 0;
        foreach (var incoming in request.Devices)
        {
            var upserted = await deviceWriter.UpsertSynchronizedDeviceAsync(incoming, cancellationToken);
            incomingIdentifiers.Add(incoming.Identifier);

            if (!existingByIdentifier.ContainsKey(incoming.Identifier))
            {
                added++;
                newlyAdded.Add(incoming);
                newlyAddedDevices.Add((incoming, upserted));
            }
            else
            {
                updated++;
            }

            if (upserted.DetectedStatus == DetectedStatus.Ignored)
                ignored++;
        }

        var removed = existingByIdentifier
            .Where(kvp => !incomingIdentifiers.Contains(kvp.Key))
            .Select(kvp => kvp.Value)
            .ToList();

        if (request.AutoAssignNewDevices ?? true)
        {
            await AutoAssignNewDevicesAsync(request.AccountId, newlyAddedDevices, cancellationToken);
        }

        var duplicates = new List<(string Serial, Guid OtherOperatorId)>();
        if (newlyAdded.Count > 0)
        {
            var dupRows = await deviceReader.FindDuplicateSerialsAsync(
                request.AccountId,
                request.OperatorId,
                newlyAdded.Select(d => d.Serial).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray(),
                cancellationToken);
            duplicates = dupRows.Select(d => (d.Serial, d.OperatorId)).ToList();
        }

        await EmitDeviceAlertsAsync(request, newlyAdded, removed, duplicates, cancellationToken);

        var finishedAt = DateTimeOffset.UtcNow;
        var triggerType = ResolveTriggerType(request.TriggerType);

        await operatorWriter.UpdateSyncSummaryAsync(request.OperatorId, finishedAt, triggerType, cancellationToken);

        // Manager no longer records the sync run: it returns the counts and Router is
        // the single writer of sync-run telemetry, recording exactly one run per attempt (success or
        // failure) with identical field completeness. OperatorSyncRunId is left empty because this VM
        // is not persisted here.
        return new OperatorSyncRunVm(
            OperatorSyncRunId: Guid.Empty,
            request.AccountId,
            request.OperatorId,
            triggerType,
            OperatorSyncResult.Succeeded,
            startedAt,
            finishedAt,
            request.Devices.Count,
            added,
            updated,
            removed.Count,
            ignored,
            PositionsRead: 0,
            PositionsAccepted: 0,
            PositionsRejected: 0,
            ErrorCode: null,
            ErrorMessage: null,
            request.CorrelationId);
    }

    private async Task EmitDeviceAlertsAsync(
        SynchronizeOperatorDevicesCommand request,
        IReadOnlyCollection<DeviceDto> newlyAdded,
        IReadOnlyCollection<DeviceVm> removed,
        IReadOnlyCollection<(string Serial, Guid OtherOperatorId)> duplicates,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (var device in newlyAdded)
            {
                await alertWriter.RecordAlertEventAsync(new AlertEventDto(
                    request.AccountId,
                    EventType: "GpsDeviceDetected",
                    Severity: "Info",
                    SourceModule: "GpsIntegration",
                    ResourceType: "SynchronizedDevice",
                    ResourceId: device.Identifier.ToString(),
                    Status: "Open",
                    PayloadJson: JsonSerializer.Serialize(new { request.OperatorId, device.Identifier, device.Serial, request.CorrelationId }),
                    DeduplicationKey: $"gps-device-detected:{device.Identifier}:{request.OperatorId:N}"),
                    cancellationToken);
            }
            foreach (var device in removed)
            {
                await alertWriter.RecordAlertEventAsync(new AlertEventDto(
                    request.AccountId,
                    EventType: "GpsDeviceRemoved",
                    Severity: "Warning",
                    SourceModule: "GpsIntegration",
                    ResourceType: "SynchronizedDevice",
                    ResourceId: device.Identifier.ToString(),
                    Status: "Open",
                    PayloadJson: JsonSerializer.Serialize(new { request.OperatorId, device.Identifier, device.Serial, request.CorrelationId }),
                    DeduplicationKey: $"gps-device-removed:{device.Identifier}:{request.OperatorId:N}"),
                    cancellationToken);
            }
            foreach (var (serial, otherOperatorId) in duplicates)
            {
                await alertWriter.RecordAlertEventAsync(new AlertEventDto(
                    request.AccountId,
                    EventType: "GpsDuplicateDeviceIdentifier",
                    Severity: "Warning",
                    SourceModule: "GpsIntegration",
                    ResourceType: "SynchronizedDevice",
                    ResourceId: serial,
                    Status: "Open",
                    PayloadJson: JsonSerializer.Serialize(new { Serial = serial, ReportedBy = request.OperatorId, AlsoOwnedBy = otherOperatorId }),
                    DeduplicationKey: $"gps-duplicate-device:{serial}:{request.AccountId:N}"),
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to emit device sync alerts for operator {OperatorId}; the sync run itself is still recorded.",
                request.OperatorId);
        }
    }

    private async Task AutoAssignNewDevicesAsync(
        Guid accountId,
        IReadOnlyCollection<(DeviceDto Incoming, DeviceVm Device)> devices,
        CancellationToken cancellationToken)
    {
        if (devices.Count == 0)
        {
            return;
        }

        var defaultGroupId = await ResolveDefaultGroupIdAsync(accountId, cancellationToken);

        foreach (var (incoming, device) in devices)
        {
            var transporter = await transporterWriter.CreateTransporterAsync(
                new TransporterDto(
                    ResolveTransporterName(incoming),
                    ResolveTransporterTypeId(incoming.DeviceTypeId),
                    accountId),
                cancellationToken);

            await assignmentWriter.AssignAsync(
                new TransporterDeviceAssignmentDto(
                    accountId,
                    transporter.TransporterId,
                    device.DeviceId,
                    Priority: 0,
                    IsPrimary: true,
                    AssignmentReason: "Initial provider sync"),
                cancellationToken);

            // Place every auto-provisioned transporter into the account's default group so plain
            // (group-scoped) users can see it on the live map. Manual group management can move it
            // later; the sync never moves it again.
            await transporterGroupWriter.CreateTransporterGroupAsync(
                new TransporterGroupDto(transporter.TransporterId, defaultGroupId),
                cancellationToken);
        }
    }

    // Resolves the account's default group by name, creating it (Active) on first use.
    private async Task<long> ResolveDefaultGroupIdAsync(Guid accountId, CancellationToken cancellationToken)
    {
        // Search by name rather than scanning the account's groups: the account read is paged, and a
        // scan of page 1 would miss an existing default group and create a duplicate every sync.
        var groups = await groupReader.GetGroupsByAccountAsync(
            accountId, 0, PageRequest.MaxPageSize, GroupMetadata.DefaultGroupName, cancellationToken);
        var existing = groups.Items.FirstOrDefault(g =>
            string.Equals(g.Name, GroupMetadata.DefaultGroupName, StringComparison.OrdinalIgnoreCase));
        if (existing.GroupId != 0)
        {
            return existing.GroupId;
        }

        var created = await groupWriter.CreateGroupAsync(
            new GroupDto(GroupMetadata.DefaultGroupName, GroupMetadata.DefaultGroupDescription, Active: true),
            accountId,
            cancellationToken);
        return created.GroupId;
    }

    private static string ResolveTransporterName(DeviceDto device)
        => FirstNonEmpty(device.ProviderDisplayName, device.Name, device.Serial, $"Device {device.Identifier}");

    private static string FirstNonEmpty(params string?[] values)
        => values.Select(v => v?.Trim()).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))!;

    private static short ResolveTransporterTypeId(short deviceTypeId)
        => (short)(Enum.IsDefined(typeof(Common.Domain.Enums.DeviceType), (int)deviceTypeId)
            ? (Common.Domain.Enums.DeviceType)deviceTypeId switch
            {
                Common.Domain.Enums.DeviceType.Aviation => Common.Domain.Enums.TransporterType.Aircraft,
                Common.Domain.Enums.DeviceType.Cycling => Common.Domain.Enums.TransporterType.Bicycle,
                Common.Domain.Enums.DeviceType.Drones => Common.Domain.Enums.TransporterType.Drone,
                Common.Domain.Enums.DeviceType.Marine => Common.Domain.Enums.TransporterType.Boat,
                Common.Domain.Enums.DeviceType.PetTracking => Common.Domain.Enums.TransporterType.Pet,
                Common.Domain.Enums.DeviceType.Phone or Common.Domain.Enums.DeviceType.Fitness or Common.Domain.Enums.DeviceType.Smartwatch or Common.Domain.Enums.DeviceType.Wearable => Common.Domain.Enums.TransporterType.Person,
                Common.Domain.Enums.DeviceType.OBDScanner => Common.Domain.Enums.TransporterType.FleetVehicle,
                _ => Common.Domain.Enums.TransporterType.Asset
            }
            : Common.Domain.Enums.TransporterType.Asset);

    private static SyncTriggerType ResolveTriggerType(string triggerType)
        => Enum.TryParse<SyncTriggerType>(triggerType, ignoreCase: true, out var parsed)
            ? parsed
            : SyncTriggerType.Automatic;
}
