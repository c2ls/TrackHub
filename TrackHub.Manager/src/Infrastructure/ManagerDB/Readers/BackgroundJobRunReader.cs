using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class BackgroundJobRunReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IBackgroundJobRunReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<IReadOnlyCollection<BackgroundJobRunVm>> GetBackgroundJobRunsAsync(Guid? accountId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken)
    {
        var scopedAccountId = ResolveAccountScope(accountId);
        return await Context.BackgroundJobRuns
            .Where(x => (!scopedAccountId.HasValue || x.AccountId == scopedAccountId) && (!from.HasValue || x.StartedAt >= from) && (!to.HasValue || x.StartedAt <= to))
            .OrderByDescending(x => x.StartedAt).ThenBy(x => x.BackgroundJobRunId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(x => new BackgroundJobRunVm(x.BackgroundJobRunId, x.JobKey, x.AccountId, x.ResourceKey, x.IdempotencyKey, x.Status, x.Attempts, x.StartedAt, x.CompletedAt, x.ErrorCode, x.ErrorMessage))
            .ToListAsync(cancellationToken);
    }
}
