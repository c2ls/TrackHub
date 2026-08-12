namespace TrackHub.Manager.Application.GpsIntegration.Queries;

[Authorize(Resource = Resources.Credentials, Action = Actions.Read)]

// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct GetOperatorCredentialMetadataQuery(Guid OperatorId) : IRequest<CredentialMetadataVm?>;

public class GetOperatorCredentialMetadataQueryHandler(ICredentialReader reader)
    : IRequestHandler<GetOperatorCredentialMetadataQuery, CredentialMetadataVm?>
{
    public Task<CredentialMetadataVm?> Handle(GetOperatorCredentialMetadataQuery request, CancellationToken cancellationToken)
        => reader.GetMetadataByOperatorAsync(request.OperatorId, cancellationToken);
}
