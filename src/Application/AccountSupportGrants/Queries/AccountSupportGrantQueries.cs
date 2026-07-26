namespace TrackHub.Manager.Application.AccountSupportGrants.Queries;

[Authorize(Resource = Resources.SupportGrants, Action = Actions.Read)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct GetSupportGrantStatusQuery(Guid AccountSupportGrantId) : IRequest<AccountSupportGrantVm>;
public class GetSupportGrantStatusQueryHandler(IAccountSupportGrantReader reader) : IRequestHandler<GetSupportGrantStatusQuery, AccountSupportGrantVm>
{
    public async Task<AccountSupportGrantVm> Handle(GetSupportGrantStatusQuery request, CancellationToken cancellationToken) => await reader.GetSupportGrantStatusAsync(request.AccountSupportGrantId, cancellationToken);
}

[Authorize(Resource = Resources.SupportGrants, Action = Actions.Read)]
[AllowCrossAccount("Platform support administration console: the whole purpose is to list support grants ACROSS accounts (AccountId is an optional filter, null = all). Gated by the SupportGrants/Read platform permission.")]
public readonly record struct GetAccountSupportGrantsQuery(Guid? AccountId = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<AccountSupportGrantVm>>;
public class GetAccountSupportGrantsQueryHandler(IAccountSupportGrantReader reader) : IRequestHandler<GetAccountSupportGrantsQuery, IReadOnlyCollection<AccountSupportGrantVm>>
{
    public async Task<IReadOnlyCollection<AccountSupportGrantVm>> Handle(GetAccountSupportGrantsQuery request, CancellationToken cancellationToken) => await reader.GetAccountSupportGrantsAsync(request.AccountId, request.Skip, request.Take, cancellationToken);
}
