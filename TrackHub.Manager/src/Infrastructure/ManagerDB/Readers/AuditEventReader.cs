using Common.Application.Interfaces;
using Common.Application.Paging;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class AuditEventReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IAuditEventReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);

    public async Task<AuditEventPageVm> GetAuditTrailAsync(
        Guid accountId, DateTimeOffset? from, DateTimeOffset? to, string? cursor, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        var pageSize = PageSize(take);

        var query = Context.AuditEvents
            .Where(x => x.AccountId == scopedAccountId && (!from.HasValue || x.OccurredAt >= from) && (!to.HasValue || x.OccurredAt <= to));

        // Strict tuple comparison against the sort key, which ends on the unique id: the seek lands
        // exactly after the last row of the previous page even when several share an instant.
        if (FeedCursor.TryDecode(cursor, out var at, out var id))
        {
            query = query.Where(x => x.OccurredAt < at || (x.OccurredAt == at && x.AuditEventId.CompareTo(id) > 0));
        }

        // One row past the page: the only thing a reader needs to know is whether to offer "more".
        var rows = await query
            .OrderByDescending(x => x.OccurredAt).ThenBy(x => x.AuditEventId)
            .Take(pageSize + 1)
            .Select(x => new AuditEventVm(x.AuditEventId, x.AccountId, x.ActorType, x.ActorId, x.Action, x.ResourceType, x.ResourceId, x.Result, x.Reason, x.IpAddress, x.UserAgent, x.CorrelationId, x.OccurredAt, null, null))
            .ToListAsync(cancellationToken);

        var hasMore = rows.Count > pageSize;
        var items = hasMore ? rows.GetRange(0, pageSize) : rows;
        var last = items.Count > 0 ? items[^1] : default;

        return new AuditEventPageVm(
            await WithNamesAsync(items, cancellationToken),
            hasMore,
            items.Count > 0 ? FeedCursor.Encode(last.OccurredAt, last.AuditEventId) : null);
    }

    // Who acted and on what, by name, for the rows of one page; an unknown or deleted id stays unnamed.
    private async Task<IReadOnlyCollection<AuditEventVm>> WithNamesAsync(List<AuditEventVm> items, CancellationToken cancellationToken)
    {
        var guids = items.SelectMany(x => new[] { x.ActorId, x.ResourceId })
            .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
        var groupIds = items.Where(x => x.ResourceType == "Group")
            .Select(x => long.TryParse(x.ResourceId, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
        if (guids.Count == 0 && groupIds.Count == 0)
        {
            return items;
        }

        var names = new Dictionary<string, string>();
        void Add(IEnumerable<(string Type, string Id, string Name)> rows)
        {
            foreach (var (type, id, name) in rows)
            {
                names[$"{type}:{id}"] = name;
            }
        }

        Add((await Context.Users.Where(u => guids.Contains(u.UserId)).Select(u => new { u.UserId, u.Username }).ToListAsync(cancellationToken))
            .Select(u => ("User", u.UserId.ToString(), u.Username)));
        Add((await Context.StaffMembers.Where(d => guids.Contains(d.StaffId)).Select(d => new { d.StaffId, d.Name }).ToListAsync(cancellationToken))
            .Select(d => ("Staff", d.StaffId.ToString(), d.Name)));
        Add((await Context.Transporters.Where(t => guids.Contains(t.TransporterId)).Select(t => new { t.TransporterId, t.Name }).ToListAsync(cancellationToken))
            .Select(t => ("Transporter", t.TransporterId.ToString(), t.Name)));
        Add((await Context.Operators.Where(o => guids.Contains(o.OperatorId)).Select(o => new { o.OperatorId, o.Name }).ToListAsync(cancellationToken))
            .Select(o => ("Operator", o.OperatorId.ToString(), o.Name)));
        Add((await Context.Documents.Where(d => guids.Contains(d.DocumentId)).Select(d => new { d.DocumentId, Name = d.Title ?? d.FileName }).ToListAsync(cancellationToken))
            .Select(d => ("Document", d.DocumentId.ToString(), d.Name)));
        Add((await Context.Groups.Where(g => groupIds.Contains(g.GroupId)).Select(g => new { g.GroupId, g.Name }).ToListAsync(cancellationToken))
            .Select(g => ("Group", g.GroupId.ToString(), g.Name)));

        return items.Select(x => x with
        {
            ActorName = names.GetValueOrDefault($"{x.ActorType}:{x.ActorId}"),
            ResourceName = names.GetValueOrDefault($"{x.ResourceType}:{x.ResourceId}"),
        }).ToList();
    }

    public async Task<IReadOnlyCollection<AuditEventVm>> GetAuditTrailByOffsetAsync(
        Guid accountId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);

        return await Context.AuditEvents
            .Where(x => x.AccountId == scopedAccountId && (!from.HasValue || x.OccurredAt >= from) && (!to.HasValue || x.OccurredAt <= to))
            .OrderByDescending(x => x.OccurredAt).ThenBy(x => x.AuditEventId)
            .Skip(Math.Max(0, skip)).Take(PageSize(take))
            .Select(x => new AuditEventVm(x.AuditEventId, x.AccountId, x.ActorType, x.ActorId, x.Action, x.ResourceType, x.ResourceId, x.Result, x.Reason, x.IpAddress, x.UserAgent, x.CorrelationId, x.OccurredAt, null, null))
            .ToListAsync(cancellationToken);
    }
}
