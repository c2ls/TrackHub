namespace TrackHub.Manager.Application.AccountSupportGrants.Commands;

// Write twin of the already-cross-account AccountSupportGrantQueries. The account is nested in
// AccountSupportGrantDto, so this side went unpoliced until TrackHubCommon 1.0.7.
[Authorize(Resource = Resources.SupportGrants, Action = Actions.Write)]
[AllowCrossAccount("Platform support administration console (portal systemadmin): a support grant authorises a platform operator to enter a CUSTOMER's account, so the target account is by definition not the grantor's own. Gated by the SupportGrants/Write platform permission.")]
public readonly record struct CreateAccountSupportGrantCommand(AccountSupportGrantDto AccountSupportGrant) : IRequest<AccountSupportGrantVm>;
public class CreateAccountSupportGrantCommandHandler(IAccountSupportGrantWriter writer) : IRequestHandler<CreateAccountSupportGrantCommand, AccountSupportGrantVm>
{
    public async Task<AccountSupportGrantVm> Handle(CreateAccountSupportGrantCommand request, CancellationToken cancellationToken) => await writer.CreateAccountSupportGrantAsync(request.AccountSupportGrant, cancellationToken);
}

[Authorize(Resource = Resources.SupportGrants, Action = Actions.Edit)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct ApproveAccountSupportGrantCommand(Guid AccountSupportGrantId, string ApprovedBy) : IRequest;
public class ApproveAccountSupportGrantCommandHandler(IAccountSupportGrantWriter writer) : IRequestHandler<ApproveAccountSupportGrantCommand>
{
    public async Task Handle(ApproveAccountSupportGrantCommand request, CancellationToken cancellationToken) => await writer.ApproveAccountSupportGrantAsync(request.AccountSupportGrantId, request.ApprovedBy, cancellationToken);
}

[Authorize(Resource = Resources.SupportGrants, Action = Actions.Delete)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct RevokeAccountSupportGrantCommand(Guid AccountSupportGrantId, string RevokedBy) : IRequest;
public class RevokeAccountSupportGrantCommandHandler(IAccountSupportGrantWriter writer) : IRequestHandler<RevokeAccountSupportGrantCommand>
{
    public async Task Handle(RevokeAccountSupportGrantCommand request, CancellationToken cancellationToken) => await writer.RevokeAccountSupportGrantAsync(request.AccountSupportGrantId, request.RevokedBy, cancellationToken);
}
