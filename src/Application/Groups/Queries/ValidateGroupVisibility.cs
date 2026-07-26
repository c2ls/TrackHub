namespace TrackHub.Manager.Application.Groups.Queries;

[Authorize(Resource = Resources.Groups, Action = Actions.Read)]
[AllowCrossAccount("Visibility probe issued by TripManagement's global trip_client service identity on behalf of a named user of the target account (UserId is on the request); the caller is a service with no account of its own. Returns a boolean.")]
public readonly record struct ValidateGroupVisibilityQuery(Guid AccountId, Guid UserId, string ResourceType, string ResourceId) : IRequest<bool>;
public class ValidateGroupVisibilityQueryHandler(IGroupVisibilityReader reader) : IRequestHandler<ValidateGroupVisibilityQuery, bool>
{
    public async Task<bool> Handle(ValidateGroupVisibilityQuery request, CancellationToken cancellationToken) => await reader.ValidateGroupVisibilityAsync(request.AccountId, request.UserId, request.ResourceType, request.ResourceId, cancellationToken);
}
