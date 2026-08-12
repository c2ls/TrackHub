namespace TrackHub.Manager.Application.BackgroundJobs.Queries;

[Authorize(Resource = Resources.BackgroundJobs, Action = Actions.Read)]
[AllowCrossAccount("Platform operations console for background-job history, which spans every account (AccountId is an optional filter, null = all) and includes platform-level runs with no account at all. Gated by the BackgroundJobs/Read platform permission.")]
public readonly record struct GetBackgroundJobRunsQuery(Guid? AccountId, DateTimeOffset? From = null, DateTimeOffset? To = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<BackgroundJobRunVm>>;
public class GetBackgroundJobRunsQueryHandler(IBackgroundJobRunReader reader) : IRequestHandler<GetBackgroundJobRunsQuery, IReadOnlyCollection<BackgroundJobRunVm>>
{
    public async Task<IReadOnlyCollection<BackgroundJobRunVm>> Handle(GetBackgroundJobRunsQuery request, CancellationToken cancellationToken) => await reader.GetBackgroundJobRunsAsync(request.AccountId, request.From, request.To, request.Skip, request.Take, cancellationToken);
}
