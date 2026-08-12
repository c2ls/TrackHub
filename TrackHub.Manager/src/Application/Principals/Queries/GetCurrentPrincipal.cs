using Common.Application.Interfaces;

namespace TrackHub.Manager.Application.Principals.Queries;

[Authorize(Resource = Resources.Profile, Action = Actions.Read)]
public readonly record struct GetCurrentPrincipalQuery() : IRequest<CurrentPrincipalVm>;

public readonly record struct CurrentPrincipalVm(string? SubjectId, PrincipalType PrincipalType, Guid? UserId, Guid? DriverId, string? ClientId, Guid? PublicLinkGrantId, string? Role, Guid? AccountId, IReadOnlyCollection<string> Scopes, IReadOnlyCollection<string> Audiences, string? CorrelationId);

public class GetCurrentPrincipalQueryHandler(ICurrentPrincipal principal) : IRequestHandler<GetCurrentPrincipalQuery, CurrentPrincipalVm>
{
    public Task<CurrentPrincipalVm> Handle(GetCurrentPrincipalQuery request, CancellationToken cancellationToken)
        => Task.FromResult(new CurrentPrincipalVm(principal.SubjectId, principal.PrincipalType, principal.UserId, principal.DriverId, principal.ClientId, principal.PublicLinkGrantId, principal.Role, principal.AccountId, principal.Scopes, principal.Audiences, principal.CorrelationId));
}
