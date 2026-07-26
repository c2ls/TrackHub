using Common.Application.Interfaces;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Writers;

public sealed class TransporterPositionHistoryWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), ITransporterPositionHistoryWriter
{
    public async Task<bool> AppendAsync(TransporterPositionHistoryDto dto, CancellationToken cancellationToken)
    {
        var scoped = RequireAccountAccess(dto.AccountId);
        var exists = await Context.TransporterPositionHistory
            .AnyAsync(x => x.AccountId == scoped && x.IdempotencyKey == dto.IdempotencyKey, cancellationToken);
        if (exists)
        {
            return false;
        }
        var entity = new TransporterPositionHistory(
            scoped, dto.OperatorId, dto.DeviceId, dto.TransporterId,
            dto.SourceTimestamp, DateTimeOffset.UtcNow,
            dto.Latitude, dto.Longitude, dto.Altitude, dto.Speed, dto.Course,
            dto.EventId, dto.Address, dto.City, dto.State, dto.Country, dto.Attributes, dto.IdempotencyKey);
        await Context.TransporterPositionHistory.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // Batched append for the Router storing pipeline. Idempotent per row; rows whose
    // DeviceId is unknown to the caller are resolved from the transporter's active
    // device assignment.
    public async Task<int> AppendRangeAsync(IReadOnlyCollection<TransporterPositionHistoryDto> dtos, CancellationToken cancellationToken)
    {
        if (dtos.Count == 0)
        {
            return 0;
        }

        var accountId = RequireAccountAccess(dtos.First().AccountId);
        var rows = dtos.Where(d => d.AccountId == accountId).ToList();

        var keys = rows.Select(d => d.IdempotencyKey).ToArray();
        var existingKeys = await Context.TransporterPositionHistory
            .Where(x => x.AccountId == accountId && keys.Contains(x.IdempotencyKey))
            .Select(x => x.IdempotencyKey)
            .ToListAsync(cancellationToken);
        var existing = existingKeys.ToHashSet();

        var unresolvedTransporterIds = rows
            .Where(d => d.DeviceId == Guid.Empty && !existing.Contains(d.IdempotencyKey))
            .Select(d => d.TransporterId)
            .Distinct()
            .ToArray();
        var deviceByTransporter = new Dictionary<Guid, Guid>();
        if (unresolvedTransporterIds.Length > 0)
        {
            var assignments = await Context.TransporterDeviceAssignments
                .Where(a => a.AccountId == accountId && a.Status == 0 && unresolvedTransporterIds.Contains(a.TransporterId))
                .Select(a => new { a.TransporterId, a.DeviceId, a.IsPrimary, a.Priority })
                .ToListAsync(cancellationToken);
            deviceByTransporter = assignments
                .GroupBy(a => a.TransporterId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.IsPrimary).ThenBy(a => a.Priority).First().DeviceId);
        }

        var appended = 0;
        foreach (var dto in rows)
        {
            if (existing.Contains(dto.IdempotencyKey))
            {
                continue;
            }

            var deviceId = dto.DeviceId != Guid.Empty
                ? dto.DeviceId
                : deviceByTransporter.GetValueOrDefault(dto.TransporterId);

            var entity = new TransporterPositionHistory(
                accountId, dto.OperatorId, deviceId, dto.TransporterId,
                dto.SourceTimestamp, DateTimeOffset.UtcNow,
                dto.Latitude, dto.Longitude, dto.Altitude, dto.Speed, dto.Course,
                dto.EventId, dto.Address, dto.City, dto.State, dto.Country, dto.Attributes, dto.IdempotencyKey);
            await Context.TransporterPositionHistory.AddAsync(entity, cancellationToken);
            existing.Add(dto.IdempotencyKey);
            appended++;
        }

        if (appended > 0)
        {
            await Context.SaveChangesAsync(cancellationToken);
        }

        return appended;
    }
}
