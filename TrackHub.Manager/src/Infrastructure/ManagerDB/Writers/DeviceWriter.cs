using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class DeviceWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IDeviceWriter
{
    public async Task<DeviceVm> UpsertSynchronizedDeviceAsync(DeviceDto deviceDto, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(deviceDto.AccountId);
        var operatorAccountId = await Context.Operators
            .Where(o => o.OperatorId == deviceDto.OperatorId)
            .Select(o => (Guid?)o.AccountId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Entities.Operator), deviceDto.OperatorId.ToString());
        if (operatorAccountId != accountId)
        {
            throw new ForbiddenAccessException();
        }

        var now = DateTimeOffset.UtcNow;

        var existing = await Context.Devices
            .FirstOrDefaultAsync(d => d.AccountId == accountId
                && d.OperatorId == deviceDto.OperatorId
                && d.Identifier == deviceDto.Identifier,
                cancellationToken);

        Entities.Device device;
        bool created;
        if (existing == null)
        {
            device = new Entities.Device(
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
            await Context.Devices.AddAsync(device, cancellationToken);
            created = true;
        }
        else
        {
            // The context is NoTracking by default; attach or the sync update never persists.
            Context.Devices.Attach(existing);
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
            device = existing;
            created = false;
        }

        AddAuditEvent(accountId, created ? "SynchronizedDevice.Created" : "SynchronizedDevice.Updated",
            "SynchronizedDevice", device.DeviceId.ToString(), null, null);
        await Context.SaveChangesAsync(cancellationToken);

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

    // Manual registration for providers without a device-catalog API (Prosegur) —
    // sync can never discover their devices, so operators enter them by hand.
    public async Task<DeviceVm> CreateManualDeviceAsync(DeviceDto deviceDto, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(deviceDto.AccountId);
        var operatorAccountId = await Context.Operators
            .Where(o => o.OperatorId == deviceDto.OperatorId)
            .Select(o => (Guid?)o.AccountId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Entities.Operator), deviceDto.OperatorId.ToString());
        if (operatorAccountId != accountId)
        {
            throw new ForbiddenAccessException();
        }

        var identifier = deviceDto.Identifier;
        if (identifier <= 0)
        {
            // Catalog-less providers supply no numeric id; allocate the next free one within
            // the (account, operator) uniqueness scope of IX_devices_accountid_operatorid_identifier.
            var max = await Context.Devices
                .Where(d => d.AccountId == accountId && d.OperatorId == deviceDto.OperatorId)
                .MaxAsync(d => (int?)d.Identifier, cancellationToken) ?? 0;
            identifier = max + 1;
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
        try
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // The unique (account, operator, identifier) index is the real guarantee: a
            // concurrent auto-allocation or an explicit id that raced past the pre-check lands
            // here. Surface it as a clean 409, not the unmapped generic error a bare
            // DbUpdateException would produce.
            Context.Devices.Entry(device).State = EntityState.Detached;
            throw new ConflictException(
                $"A device with identifier {identifier} already exists for this operator.");
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

        RequireAccountWriteAccess(device.AccountId);

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
            "SynchronizedDevice", deviceId.ToString(), null, $"{{\"status\":\"{status}\"}}");
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteDeviceAsync(Guid deviceId, CancellationToken cancellationToken)
    {
        var device = await Context.Devices.FindAsync([deviceId], cancellationToken)
            ?? throw new NotFoundException(nameof(Entities.Device), deviceId.ToString());

        RequireAccountWriteAccess(device.AccountId);
        Context.Devices.Remove(device);
        AddAuditEvent(device.AccountId, "SynchronizedDevice.Deleted",
            "SynchronizedDevice", deviceId.ToString(), null, null);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DeleteDevicesByOperatorAsync(Guid operatorId, CancellationToken cancellationToken)
    {
        var devices = await Context.Devices
            .Where(d => d.OperatorId == operatorId
                && (CanAccessAllAccounts || d.AccountId == Principal.AccountId))
            .ToListAsync(cancellationToken);

        if (devices.Count == 0) return 0;

        var deviceIds = devices.Select(d => d.DeviceId).ToHashSet();
        var transporterIdsWithWipedDevices = await Context.TransporterDeviceAssignments
            .Where(a => deviceIds.Contains(a.DeviceId))
            .Select(a => a.TransporterId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var transportersToDelete = await Context.Transporters
            .Where(t => transporterIdsWithWipedDevices.Contains(t.TransporterId)
                && !t.Assignments.Any(a => !deviceIds.Contains(a.DeviceId)))
            .ToListAsync(cancellationToken);
        var transporterIdsToDelete = transportersToDelete.Select(t => t.TransporterId).ToHashSet();

        if (transporterIdsToDelete.Count > 0)
        {
            var positions = await Context.TransporterPositions
                .Where(p => transporterIdsToDelete.Contains(p.TransporterId))
                .ToListAsync(cancellationToken);
            Context.TransporterPositions.RemoveRange(positions);
        }

        foreach (var d in devices)
        {
            Context.Devices.Remove(d);
            AddAuditEvent(d.AccountId, "SynchronizedDevice.Wiped",
                "SynchronizedDevice", d.DeviceId.ToString(), null, null);
        }
        foreach (var t in transportersToDelete)
        {
            Context.Transporters.Remove(t);
            AddAuditEvent(t.AccountId, "Transporter.WipedWithSynchronizedDevices",
                "Transporter", t.TransporterId.ToString(), null, $"{{\"operatorId\":\"{operatorId}\"}}");
        }
        await Context.SaveChangesAsync(cancellationToken);
        return devices.Count;
    }
}
