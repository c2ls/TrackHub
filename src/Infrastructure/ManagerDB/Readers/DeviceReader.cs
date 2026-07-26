using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Enums;
using Common.Domain.Helpers;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class DeviceReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IDeviceReader
{
    private static readonly System.Linq.Expressions.Expression<Func<Entities.Device, DeviceVm>> Projection = d => new DeviceVm(
        d.DeviceId,
        d.AccountId,
        d.OperatorId,
        d.Serial,
        d.Name,
        d.Identifier,
        d.ProviderDisplayName,
        (DeviceType)d.DeviceTypeId,
        d.DeviceTypeId,
        d.Description,
        d.ProviderMetadataHash,
        d.ProviderStatus,
        (DetectedStatus)(
            d.DetectedStatus == (int)DetectedStatus.Ignored
                ? (int)DetectedStatus.Ignored
                : d.DetectedStatus == (int)DetectedStatus.Removed
                    ? (int)DetectedStatus.Removed
                    : d.Assignments.Any(a => a.Status == (int)AssignmentStatus.Active)
                        ? (int)DetectedStatus.Assigned
                        : (int)DetectedStatus.Available),
        d.FirstSeenAt,
        d.LastSeenAt,
        d.LastSyncedAt,
        d.LastAssignedAt,
        d.IgnoredAt);

    public async Task<DeviceVm> GetDeviceAsync(Guid id, CancellationToken cancellationToken)
    {
        var device = await Context.Devices
            .Where(d => d.DeviceId == id)
            .Select(Projection)
            .Cast<DeviceVm?>()
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Entities.Device), id.ToString());

        RequireAccountAccess(device.AccountId);
        return device;
    }

    public async Task<DevicesPageVm> GetDevicesByAccountAsync(Guid accountId, int skip, int take, string? search, CancellationToken cancellationToken)
    {
        var scoped = RequireAccountAccess(accountId);
        return await PageAsync(
            ApplySearch(Context.Devices.Where(d => d.AccountId == scoped), search), skip, take, cancellationToken);
    }

    public async Task<IReadOnlyCollection<DeviceVm>> GetDevicesByOperatorAsync(Guid operatorId, CancellationToken cancellationToken)
    {
        return await Context.Devices
            .Where(d => d.OperatorId == operatorId
                && (CanAccessAllAccounts || d.AccountId == Principal.AccountId))
            .Select(Projection)
            .ToListAsync(cancellationToken);
    }

    public async Task<DevicesPageVm> GetUnassignedDevicesAsync(Guid accountId, int skip, int take, string? search, CancellationToken cancellationToken)
    {
        var scoped = RequireAccountAccess(accountId);
        var query = Context.Devices
            .Where(d => d.AccountId == scoped
                && d.IgnoredAt == null
                && !d.Assignments.Any(a => a.Status == (int)AssignmentStatus.Active));
        return await PageAsync(ApplySearch(query, search), skip, take, cancellationToken);
    }

    public async Task<DevicesPageVm> SearchDevicesAsync(Guid accountId, SynchronizedDeviceFilter filter, int skip, int take, string? search, CancellationToken cancellationToken)
    {
        var scoped = RequireAccountAccess(accountId);
        var query = Context.Devices.Where(d => d.AccountId == scoped);

        query = ApplyDetectedStatus(query, filter.DetectedStatus);

        if (filter.OperatorId.HasValue)
        {
            var operatorId = filter.OperatorId.Value;
            query = query.Where(d => d.OperatorId == operatorId);
        }

        // "Unassigned only" is the negation of the projection's Assigned branch — every status
        // except Assigned survives, which is wider than picking one status from the dropdown.
        if (filter.UnassignedOnly)
        {
            query = query.Where(d => d.DetectedStatus == (int)DetectedStatus.Ignored
                || d.DetectedStatus == (int)DetectedStatus.Removed
                || !d.Assignments.Any(a => a.Status == (int)AssignmentStatus.Active));
        }

        if (filter.FirstSeenSince.HasValue)
        {
            var since = filter.FirstSeenSince.Value;
            query = query.Where(d => d.FirstSeenAt >= since);
        }

        return await PageAsync(ApplySearch(query, search), skip, take, cancellationToken);
    }

    public async Task<IReadOnlyCollection<DeviceLookupVm>> GetDeviceLookupByAccountAsync(Guid accountId, int fetchSize, CancellationToken cancellationToken)
    {
        var scoped = RequireAccountAccess(accountId);
        return await Context.Devices
            .Where(d => d.AccountId == scoped)
            .OrderBy(d => d.Name)
            .ThenBy(d => d.DeviceId)
            .Take(fetchSize)
            .Select(d => new DeviceLookupVm(d.DeviceId, d.Name, d.OperatorId))
            .ToListAsync(cancellationToken);
    }

    // The stored DetectedStatus column only ever holds Ignored or Removed; Assigned and Available
    // are computed from the active assignments in Projection. Comparing the column would therefore
    // return nothing for the two statuses operators pick most, so each branch is rebuilt here to
    // match the projection exactly.
    private static IQueryable<Entities.Device> ApplyDetectedStatus(
        IQueryable<Entities.Device> query, DetectedStatus? detectedStatus)
        => detectedStatus switch
        {
            null => query,
            DetectedStatus.Ignored => query.Where(d => d.DetectedStatus == (int)DetectedStatus.Ignored),
            DetectedStatus.Removed => query.Where(d => d.DetectedStatus == (int)DetectedStatus.Removed),
            DetectedStatus.Assigned => query.Where(d => d.DetectedStatus != (int)DetectedStatus.Ignored
                && d.DetectedStatus != (int)DetectedStatus.Removed
                && d.Assignments.Any(a => a.Status == (int)AssignmentStatus.Active)),
            DetectedStatus.Available => query.Where(d => d.DetectedStatus != (int)DetectedStatus.Ignored
                && d.DetectedStatus != (int)DetectedStatus.Removed
                && !d.Assignments.Any(a => a.Status == (int)AssignmentStatus.Active)),
            // New is a seed value only: Projection resolves a stored New row to Available or
            // Assigned, so no row can ever carry it and an empty page is the honest answer.
            _ => query.Where(d => false),
        };

    // Every paged device read shares one total order: Name, then the primary key as tiebreaker.
    // Without the tiebreaker, two devices with the same name make Skip/Take repeat one row and drop
    // another between pages.
    private static async Task<DevicesPageVm> PageAsync(
        IQueryable<Entities.Device> query, int skip, int take, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(d => d.Name)
            .ThenBy(d => d.DeviceId)
            .Skip(skip)
            .Take(take)
            .Select(Projection)
            .ToListAsync(cancellationToken);

        return new DevicesPageVm(items, totalCount);
    }

    private static IQueryable<Entities.Device> ApplySearch(IQueryable<Entities.Device> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = SearchPattern.Contains(search);
        return query.Where(d => EF.Functions.ILike(d.Name, term, SearchPattern.Escape)
            || EF.Functions.ILike(d.Serial, term, SearchPattern.Escape));
    }

    public async Task<IReadOnlyCollection<(string Serial, Guid OperatorId)>> FindDuplicateSerialsAsync(
        Guid accountId, Guid excludeOperatorId, IReadOnlyCollection<string> serials, CancellationToken cancellationToken)
    {
        if (serials is null || serials.Count == 0)
            return [];

        var scoped = RequireAccountAccess(accountId);
        var serialSet = serials.Where(s => !string.IsNullOrWhiteSpace(s)).ToHashSet(StringComparer.Ordinal);
        if (serialSet.Count == 0)
            return [];

        var rows = await Context.Devices
            .Where(d => d.AccountId == scoped
                && d.OperatorId != excludeOperatorId
                && serialSet.Contains(d.Serial))
            .Select(d => new { d.Serial, d.OperatorId })
            .ToListAsync(cancellationToken);

        return rows.Select(r => (r.Serial, r.OperatorId)).ToList();
    }
}
