using Common.Domain.Constants;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

// Deliberately platform-scoped (unscoped): the administrator tier of the status page needs the
// latest run per JobKey across ALL accounts, including the null-AccountId platform jobs. It does
// not derive from AccountScopedDataAccess; [Authorize(Administrative, Read)] is the gate.
public sealed class BackgroundJobStatusReader(IApplicationDbContext context) : IBackgroundJobStatusReader
{
    public async Task<IReadOnlyCollection<BackgroundJobStatusVm>> GetBackgroundJobStatusAsync(CancellationToken cancellationToken)
    {
        // Deliberately two round trips instead of one grouped query.
        //
        // The natural single-query form — GroupBy(JobKey).Select(g => g.OrderByDescending(...).First())
        // — is NOT reliably translatable by EF Core, and the EF InMemory provider used by the unit
        // tests happily evaluates it client-side, so a translation failure would only ever surface
        // against real PostgreSQL in production (the same trap rules.md documents for json columns).
        // The key set is tiny and bounded (one row per BackgroundJobKeys entry, ~9 today), and this
        // is an Administrator-only, uncached screen, so the extra round trips are irrelevant.
        //
        // Distinct() here is over a scalar string projection, not an entity, so the Postgres
        // json-equality restriction does not apply.
        var jobKeys = await context.BackgroundJobRuns
            .Select(x => x.JobKey)
            .Distinct()
            .ToListAsync(cancellationToken);

        var statuses = new List<BackgroundJobStatusVm>(jobKeys.Count);
        foreach (var jobKey in jobKeys)
        {
            var latest = await context.BackgroundJobRuns
                .Where(x => x.JobKey == jobKey)
                .OrderByDescending(x => x.StartedAt).ThenByDescending(x => x.BackgroundJobRunId)
                .Select(x => new BackgroundJobStatusVm(
                    x.JobKey, x.Status, x.StartedAt, x.CompletedAt, x.Attempts, x.ErrorCode,
                    // Constant per key; evaluated client-side after the projection materializes.
                    false))
                .FirstOrDefaultAsync(cancellationToken);

            if (latest.JobKey is not null)
            {
                statuses.Add(latest with { RecordsEveryCycle = BackgroundJobKeys.IsCycleRecorded(latest.JobKey) });
            }
        }

        return [.. statuses.OrderBy(x => x.JobKey, StringComparer.OrdinalIgnoreCase)];
    }
}
