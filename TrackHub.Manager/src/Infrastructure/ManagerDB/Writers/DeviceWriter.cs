using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Infrastructure;
using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class DeviceWriter(IApplicationDbContext context, ICurrentPrincipal principal, IOperatorCatalogGate catalogGate)
    : AccountScopedDataAccess(context, principal), IDeviceWriter
{
    /// <summary>
    /// Upserts a whole provider catalog in ONE unit of work: the operator is validated once, the
    /// operator's devices are loaded once, every row is mutated in memory, and a single summary
    /// audit event is written. Per-device it issued an operator lookup, a device lookup, an audit
    /// row and its own SaveChangesAsync — roughly 3 000 round trips and 2 000 audit rows for a
    /// 1 000-device operator, with no rollback if it failed half way.
    /// </summary>
    // One save for the whole catalog: upserts, revivals, and retirements of devices the provider no
    // longer lists (their active assignments end here, so the transporter keeps its id and history).
    public Task<DeviceReconciliationVm> ReconcileSynchronizedDevicesAsync(
        Guid operatorId, IReadOnlyCollection<DeviceDto> devices, bool resetDeviceCatalog, CancellationToken cancellationToken)
        => catalogGate.RunAsync(operatorId, () => ReconcileAsync(operatorId, devices, resetDeviceCatalog, cancellationToken), cancellationToken);

    private async Task<DeviceReconciliationVm> ReconcileAsync(
        Guid operatorId, IReadOnlyCollection<DeviceDto> devices, bool resetDeviceCatalog, CancellationToken cancellationToken)
    {
        var accountId = await Context.Operators
            .Where(o => o.OperatorId == operatorId)
            .Select(o => (Guid?)o.AccountId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Entities.Operator), operatorId.ToString());
        RequireAccountWriteAccess(accountId);

        if (devices.Any(d => d.AccountId != accountId || d.OperatorId != operatorId))
        {
            throw new ForbiddenAccessException();
        }

        var existing = await Context.Devices
            .AsTracking()
            .Include(d => d.Assignments.Where(a => a.Status == (int)AssignmentStatus.Active))
            .Where(d => d.AccountId == accountId && d.OperatorId == operatorId)
            .ToListAsync(cancellationToken);
        var existingByIdentifier = existing.ToDictionary(d => d.Identifier);
        // Providers that identify devices only by a string send no numeric identifier: those devices
        // are matched by serial and take the operator's next free identifier when first seen.
        var identifierBySerial = existing
            .GroupBy(d => d.Serial)
            .ToDictionary(g => g.Key, g => g.First().Identifier);
        var nextIdentifier = existing.Select(d => d.Identifier).Concat(devices.Select(d => d.Identifier)).Append(0).Max();

        if (devices.Count == 0 && existing.Exists(d => d.DetectedStatus != (int)DetectedStatus.Removed))
        {
            throw new ConflictException("The provider returned an empty device catalog for an operator that has synchronized devices; nothing was changed.");
        }

        var now = DateTimeOffset.UtcNow;
        var results = new List<DeviceVm>(devices.Count);
        var added = new List<DeviceVm>();
        var incoming = new HashSet<int>();

        foreach (var providerDevice in devices)
        {
            var deviceDto = providerDevice;
            if (deviceDto.Identifier <= 0)
            {
                if (!identifierBySerial.TryGetValue(deviceDto.Serial, out var identifier))
                {
                    identifier = ++nextIdentifier;
                    identifierBySerial[deviceDto.Serial] = identifier;
                }

                deviceDto = deviceDto with { Identifier = identifier };
            }

            incoming.Add(deviceDto.Identifier);
            Entities.Device device;
            if (existingByIdentifier.TryGetValue(deviceDto.Identifier, out var known))
            {
                var revived = known.DetectedStatus == (int)DetectedStatus.Removed;
                if (revived || (resetDeviceCatalog && known.DetectedStatus == (int)DetectedStatus.Ignored))
                {
                    known.DetectedStatus = (int)DetectedStatus.New;
                    known.RemovedAt = null;
                    known.IgnoredAt = null;
                }

                Apply(known, deviceDto, now);
                device = known;
                if (revived)
                {
                    added.Add(ToVm(device));
                }
            }
            else
            {
                device = NewSynchronizedDevice(deviceDto, accountId, now);
                await Context.Devices.AddAsync(device, cancellationToken);
                existingByIdentifier[deviceDto.Identifier] = device;
                added.Add(ToVm(device));
            }

            results.Add(ToVm(device));
        }

        var retired = new List<DeviceVm>();
        foreach (var missing in existing.Where(d => !incoming.Contains(d.Identifier) && d.DetectedStatus != (int)DetectedStatus.Removed))
        {
            missing.DetectedStatus = (int)DetectedStatus.Removed;
            missing.RemovedAt = now;
            foreach (var assignment in missing.Assignments.Where(a => a.Status == (int)AssignmentStatus.Active))
            {
                assignment.Status = (int)AssignmentStatus.Ended;
                assignment.EffectiveTo = now;
                assignment.AssignmentReason = "Removed from provider catalog";
            }

            retired.Add(ToVm(missing));
        }

        AddAuditEvent(accountId, "SynchronizedDevice.Synced", "SynchronizedDevice", operatorId.ToString(), null,
            AuditJson.Of(new { devices = devices.Count, added = added.Count, retired = retired.Count, reset = resetDeviceCatalog }));

        await Context.SaveChangesAsync(cancellationToken);

        return new DeviceReconciliationVm(results, added, retired);
    }

    private static Entities.Device NewSynchronizedDevice(DeviceDto deviceDto, Guid accountId, DateTimeOffset now)
        => new(
            deviceDto.Name,
            deviceDto.Identifier,
            deviceDto.Serial,
            deviceDto.DeviceTypeId,
            deviceDto.Description,
            deviceDto.ProviderDisplayName,
            deviceDto.ProviderMetadataHash,
            deviceDto.ProviderStatus,
            (int)DetectedStatus.New,
            deviceDto.OperatorId,
            accountId)
        {
            FirstSeenAt = now,
            LastSeenAt = now,
            LastSyncedAt = now
        };

    private static void Apply(Entities.Device existing, DeviceDto deviceDto, DateTimeOffset now)
    {
        existing.Name = deviceDto.Name;
        existing.Identifier = deviceDto.Identifier;
        existing.Serial = deviceDto.Serial;
        existing.DeviceTypeId = deviceDto.DeviceTypeId;
        existing.Description = deviceDto.Description;
        existing.ProviderDisplayName = deviceDto.ProviderDisplayName;
        existing.ProviderMetadataHash = deviceDto.ProviderMetadataHash;
        existing.ProviderStatus = deviceDto.ProviderStatus;
        existing.LastSeenAt = now;
        existing.LastSyncedAt = now;
        if (existing.DetectedStatus == (int)DetectedStatus.New)
        {
            existing.DetectedStatus = (int)DetectedStatus.Available;
        }
    }

    private static DeviceVm ToVm(Entities.Device device)
        => new(
            device.DeviceId,
            device.AccountId,
            device.OperatorId,
            device.Serial,
            device.Name,
            device.Identifier,
            device.ProviderDisplayName,
            (DeviceType)device.DeviceTypeId,
            device.DeviceTypeId,
            device.Description,
            device.ProviderMetadataHash,
            device.ProviderStatus,
            (DetectedStatus)device.DetectedStatus,
            device.FirstSeenAt,
            device.LastSeenAt,
            device.LastSyncedAt,
            device.LastAssignedAt,
            device.IgnoredAt);

    private const int MaxIdentifierAllocationRetries = 3;
    private const string IdentifierIndex = "IX_devices_accountid_operatorid_identifier";

    private async Task<int> NextIdentifierAsync(Guid accountId, Guid operatorId, CancellationToken cancellationToken)
        => (await Context.Devices
            .Where(d => d.AccountId == accountId && d.OperatorId == operatorId)
            .MaxAsync(d => (int?)d.Identifier, cancellationToken) ?? 0) + 1;

    // Manual registration for providers without a device-catalog API (Prosegur) —
    // sync can never discover their devices, so operators enter them by hand.
    public async Task<DeviceVm> CreateManualDeviceAsync(DeviceDto deviceDto, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(deviceDto.AccountId);
        if (!await Context.Operators.AnyAsync(o => o.OperatorId == deviceDto.OperatorId && o.AccountId == accountId, cancellationToken))
        {
            throw new NotFoundException(nameof(Entities.Operator), deviceDto.OperatorId.ToString());
        }

        var allocated = deviceDto.Identifier <= 0;
        var identifier = deviceDto.Identifier;
        if (allocated)
        {
            identifier = await NextIdentifierAsync(accountId, deviceDto.OperatorId, cancellationToken);
        }
        else
        {
            var taken = await Context.Devices.AnyAsync(
                d => d.AccountId == accountId
                    && d.OperatorId == deviceDto.OperatorId
                    && d.Identifier == identifier,
                cancellationToken);
            if (taken)
            {
                throw new ConflictException($"A device with identifier {identifier} already exists for this operator.");
            }
        }

        var now = DateTimeOffset.UtcNow;
        var device = new Entities.Device(
            deviceDto.Name,
            identifier,
            deviceDto.Serial,
            deviceDto.DeviceTypeId,
            deviceDto.Description,
            // A manually registered device has no provider catalog behind it, so it carries no
            // provider metadata — ignore any client-supplied values rather than persist (and
            // risk over-length) fields that only the sync path legitimately fills.
            providerDisplayName: null,
            providerMetadataHash: null,
            providerStatus: null,
            (int)DetectedStatus.New,
            deviceDto.OperatorId,
            accountId)
        {
            FirstSeenAt = now,
            LastSeenAt = now,
            LastSyncedAt = now
        };
        await Context.Devices.AddAsync(device, cancellationToken);

        AddAuditEvent(accountId, "SynchronizedDevice.CreatedManually",
            "SynchronizedDevice", device.DeviceId.ToString(), null, null);
        // The unique (account, operator, identifier) index is the real guarantee; the pre-check is
        // only a friendlier message. An ALLOCATED identifier retries on a collision — manual
        // registration allocates every time, so two concurrent registrations are ordinary and must
        // not surface as a conflict the operator cannot act on.
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await Context.SaveChangesAsync(cancellationToken);
                break;
            }
            catch (DbUpdateException exception) when (allocated && attempt < MaxIdentifierAllocationRetries && UniqueViolation.Matches(exception, IdentifierIndex))
            {
                identifier = await NextIdentifierAsync(accountId, deviceDto.OperatorId, cancellationToken);
                device.Identifier = identifier;
            }
            catch (DbUpdateException exception) when (UniqueViolation.Matches(exception, IdentifierIndex))
            {
                Context.Devices.Entry(device).State = EntityState.Detached;
                throw new ConflictException(
                    $"A device with identifier {identifier} already exists for this operator.");
            }
        }

        return new DeviceVm(
            device.DeviceId,
            device.AccountId,
            device.OperatorId,
            device.Serial,
            device.Name,
            device.Identifier,
            device.ProviderDisplayName,
            (DeviceType)device.DeviceTypeId,
            device.DeviceTypeId,
            device.Description,
            device.ProviderMetadataHash,
            device.ProviderStatus,
            (DetectedStatus)device.DetectedStatus,
            device.FirstSeenAt,
            device.LastSeenAt,
            device.LastSyncedAt,
            device.LastAssignedAt,
            device.IgnoredAt);
    }

    public async Task SetDetectedStatusAsync(Guid deviceId, DetectedStatus status, CancellationToken cancellationToken)
    {
        var device = await Context.Devices.FindAsync([deviceId], cancellationToken)
            ?? throw new NotFoundException(nameof(Entities.Device), deviceId.ToString());

        RequireRowAccess(device.AccountId, nameof(Entities.Device), deviceId.ToString(), forWrite: true);

        Context.Devices.Attach(device);
        device.DetectedStatus = (int)status;
        if (status == DetectedStatus.Ignored)
        {
            device.IgnoredAt = DateTimeOffset.UtcNow;
        }
        else if (device.IgnoredAt.HasValue && status != DetectedStatus.Ignored)
        {
            device.IgnoredAt = null;
        }
        AddAuditEvent(device.AccountId, "SynchronizedDevice.StatusChanged",
            "SynchronizedDevice", deviceId.ToString(), null, AuditJson.Of(new { status = status.ToString() }));
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteDeviceAsync(Guid deviceId, CancellationToken cancellationToken)
    {
        var device = await Context.Devices.FindAsync([deviceId], cancellationToken)
            ?? throw new NotFoundException(nameof(Entities.Device), deviceId.ToString());

        RequireRowAccess(device.AccountId, nameof(Entities.Device), deviceId.ToString(), forWrite: true);
        Context.Devices.Remove(device);
        AddAuditEvent(device.AccountId, "SynchronizedDevice.Deleted",
            "SynchronizedDevice", deviceId.ToString(), null, null);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
