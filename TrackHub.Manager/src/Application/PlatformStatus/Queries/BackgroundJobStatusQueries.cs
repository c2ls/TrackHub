namespace TrackHub.Manager.Application.PlatformStatus.Queries;

// Platform-wide (unscoped) latest run per JobKey for the administrator tier of the status page.
// Administrative is Administrator-only by the seeded grants. No [Caching]: freshness is the point.
[Authorize(Resource = Resources.Administrative, Action = Actions.Read)]
public readonly record struct GetBackgroundJobStatusQuery() : IRequest<IReadOnlyCollection<BackgroundJobStatusVm>>;
public class GetBackgroundJobStatusQueryHandler(IBackgroundJobStatusReader reader) : IRequestHandler<GetBackgroundJobStatusQuery, IReadOnlyCollection<BackgroundJobStatusVm>>
{
    public async Task<IReadOnlyCollection<BackgroundJobStatusVm>> Handle(GetBackgroundJobStatusQuery request, CancellationToken cancellationToken)
        => await reader.GetBackgroundJobStatusAsync(cancellationToken);
}
