namespace TrackHub.Manager.Application.PlatformStatus.Queries;

// Administrative is held only by the Administrator role in the seeded grants, so this resource is
// what makes the announcement management surface SuperAdministrator-only. No [Caching]: freshness
// matters and the volume is trivial.
[Authorize(Resource = Resources.Administrative, Action = Actions.Read)]
public readonly record struct GetPlatformAnnouncementsQuery(int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<PlatformAnnouncementVm>>;
public class GetPlatformAnnouncementsQueryHandler(IPlatformAnnouncementReader reader) : IRequestHandler<GetPlatformAnnouncementsQuery, IReadOnlyCollection<PlatformAnnouncementVm>>
{
    public async Task<IReadOnlyCollection<PlatformAnnouncementVm>> Handle(GetPlatformAnnouncementsQuery request, CancellationToken cancellationToken)
        => await reader.GetPlatformAnnouncementsAsync(request.Skip, request.Take, cancellationToken);
}
