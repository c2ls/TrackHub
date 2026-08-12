namespace TrackHub.Manager.Application.Documents.Queries;

// Per-owner document panel (ungated — gated by the owning module's feature). Now applies owner group
// visibility + classification filtering, not tenant scope alone.
[Authorize(Resource = Resources.Documents, Action = Actions.Read)]
public readonly record struct GetDocumentsForOwnerQuery(Guid AccountId, string OwnerEntityType, string OwnerEntityId, DateTimeOffset? From = null, DateTimeOffset? To = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<DocumentVm>>;
public class GetDocumentsForOwnerQueryHandler(IDocumentReader reader) : IRequestHandler<GetDocumentsForOwnerQuery, IReadOnlyCollection<DocumentVm>>
{
    public async Task<IReadOnlyCollection<DocumentVm>> Handle(GetDocumentsForOwnerQuery request, CancellationToken cancellationToken) => await reader.GetDocumentsForOwnerAsync(request.AccountId, request.OwnerEntityType, request.OwnerEntityId, request.From, request.To, request.Skip, request.Take, cancellationToken);
}

// Drivers download their assigned documents via this query (assignment enforced in the reader).
[Authorize(Resource = Resources.Documents, Action = Actions.Read, PrincipalTypes = "User,Driver,ServiceClient")]
[AllowCrossAccount("TripManagement's DocumentClient fetches POD documents under the global trip_client identity with no account claim. User/driver callers are NOT unguarded by this: DocumentReader.GetDocumentAsync loads the row and RequireAccountAccess checks its owning account (drivers additionally by assignment).")]
public readonly record struct GetDocumentQuery(Guid DocumentId) : IRequest<DocumentVm>;
public class GetDocumentQueryHandler(IDocumentReader reader) : IRequestHandler<GetDocumentQuery, DocumentVm>
{
    public async Task<DocumentVm> Handle(GetDocumentQuery request, CancellationToken cancellationToken) => await reader.GetDocumentAsync(request.DocumentId, cancellationToken);
}

[Authorize(Resource = Resources.Documents, Action = Actions.Read)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct GetDocumentVersionsQuery(Guid DocumentId, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<DocumentVersionVm>>;
public class GetDocumentVersionsQueryHandler(IDocumentReader reader) : IRequestHandler<GetDocumentVersionsQuery, IReadOnlyCollection<DocumentVersionVm>>
{
    public async Task<IReadOnlyCollection<DocumentVersionVm>> Handle(GetDocumentVersionsQuery request, CancellationToken cancellationToken) => await reader.GetDocumentVersionsAsync(request.DocumentId, request.Skip, request.Take, cancellationToken);
}

[Authorize(Resource = Resources.Documents, Action = Actions.Read)]
public readonly record struct GetActiveDocumentByCategoryQuery(string OwnerEntityType, string OwnerEntityId, string Category) : IRequest<DocumentVm?>;
public class GetActiveDocumentByCategoryQueryHandler(IDocumentReader reader) : IRequestHandler<GetActiveDocumentByCategoryQuery, DocumentVm?>
{
    public async Task<DocumentVm?> Handle(GetActiveDocumentByCategoryQuery request, CancellationToken cancellationToken) => await reader.GetActiveDocumentByCategoryAsync(request.OwnerEntityType, request.OwnerEntityId, request.Category, cancellationToken);
}

[Authorize(Resource = Resources.Documents, Action = Actions.Read)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct GetDocumentSignaturesQuery(Guid DocumentId) : IRequest<IReadOnlyCollection<DocumentSignatureVm>>;
public class GetDocumentSignaturesQueryHandler(IDocumentReader reader) : IRequestHandler<GetDocumentSignaturesQuery, IReadOnlyCollection<DocumentSignatureVm>>
{
    public async Task<IReadOnlyCollection<DocumentSignatureVm>> Handle(GetDocumentSignaturesQuery request, CancellationToken cancellationToken) => await reader.GetDocumentSignaturesAsync(request.DocumentId, cancellationToken);
}

// Standalone Document Management surfaces are gated by the `documents` feature.
[Authorize(Resource = Resources.Documents, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Documents)]
public readonly record struct SearchDocumentsQuery(DocumentSearchFilter Filter, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<DocumentVm>>;
public class SearchDocumentsQueryHandler(IDocumentReader reader) : IRequestHandler<SearchDocumentsQuery, IReadOnlyCollection<DocumentVm>>
{
    public async Task<IReadOnlyCollection<DocumentVm>> Handle(SearchDocumentsQuery request, CancellationToken cancellationToken) => await reader.SearchDocumentsAsync(request.Filter, request.Skip, request.Take, cancellationToken);
}

[Authorize(Resource = Resources.Documents, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Documents)]
public readonly record struct GetExpiringDocumentsQuery(int WithinDays = 30, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<DocumentVm>>;
public class GetExpiringDocumentsQueryHandler(IDocumentReader reader) : IRequestHandler<GetExpiringDocumentsQuery, IReadOnlyCollection<DocumentVm>>
{
    public async Task<IReadOnlyCollection<DocumentVm>> Handle(GetExpiringDocumentsQuery request, CancellationToken cancellationToken) => await reader.GetExpiringDocumentsAsync(request.WithinDays, request.Skip, request.Take, cancellationToken);
}

[Authorize(Resource = Resources.Documents, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Documents)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct GetDocumentSharesQuery(Guid DocumentId) : IRequest<IReadOnlyCollection<PublicLinkGrantVm>>;
public class GetDocumentSharesQueryHandler(IDocumentReader reader) : IRequestHandler<GetDocumentSharesQuery, IReadOnlyCollection<PublicLinkGrantVm>>
{
    public async Task<IReadOnlyCollection<PublicLinkGrantVm>> Handle(GetDocumentSharesQuery request, CancellationToken cancellationToken) => await reader.GetDocumentSharesAsync(request.DocumentId, cancellationToken);
}

[Authorize(Resource = Resources.Documents, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Documents)]
public readonly record struct GetDocumentTypesQuery(Guid AccountId, bool IncludeDisabled = false) : IRequest<IReadOnlyCollection<DocumentTypeVm>>;
public class GetDocumentTypesQueryHandler(IDocumentReader reader) : IRequestHandler<GetDocumentTypesQuery, IReadOnlyCollection<DocumentTypeVm>>
{
    public async Task<IReadOnlyCollection<DocumentTypeVm>> Handle(GetDocumentTypesQuery request, CancellationToken cancellationToken) => await reader.GetDocumentTypesAsync(request.AccountId, request.IncludeDisabled, cancellationToken);
}

// Batched compliance read for the missing-required-documents report: every group-visible
// transporter with its Active document categories in one call (replaces one documentsForOwner
// call per transporter).
[Authorize(Resource = Resources.Documents, Action = Actions.Read)]
[RequireFeature(FeatureKeys.Documents)]
public readonly record struct GetTransporterDocumentComplianceQuery(Guid AccountId) : IRequest<IReadOnlyCollection<TransporterDocumentComplianceVm>>;
public class GetTransporterDocumentComplianceQueryHandler(IDocumentReader reader) : IRequestHandler<GetTransporterDocumentComplianceQuery, IReadOnlyCollection<TransporterDocumentComplianceVm>>
{
    public async Task<IReadOnlyCollection<TransporterDocumentComplianceVm>> Handle(GetTransporterDocumentComplianceQuery request, CancellationToken cancellationToken) => await reader.GetTransporterDocumentComplianceAsync(request.AccountId, cancellationToken);
}
