using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class BackgroundJobRunWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IBackgroundJobRunWriter
{
    public async Task<BackgroundJobRunVm> CreateBackgroundJobRunAsync(BackgroundJobRunDto backgroundJobRun, CancellationToken cancellationToken)
    {
        Guid? accountId = backgroundJobRun.AccountId.HasValue
            ? RequireAccountWriteAccess(backgroundJobRun.AccountId.Value)
            : CanAccessAllAccounts ? null : throw new ForbiddenAccessException();
        var entity = new BackgroundJobRun(backgroundJobRun.JobKey, accountId, backgroundJobRun.ResourceKey, backgroundJobRun.IdempotencyKey, backgroundJobRun.Status, backgroundJobRun.Attempts, backgroundJobRun.StartedAt)
        {
            CompletedAt = backgroundJobRun.CompletedAt,
            ErrorCode = backgroundJobRun.ErrorCode,
            ErrorMessage = backgroundJobRun.ErrorMessage
        };
        await Context.BackgroundJobRuns.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return new BackgroundJobRunVm(entity.BackgroundJobRunId, entity.JobKey, entity.AccountId, entity.ResourceKey, entity.IdempotencyKey, entity.Status, entity.Attempts, entity.StartedAt, entity.CompletedAt, entity.ErrorCode, entity.ErrorMessage);
    }
}
