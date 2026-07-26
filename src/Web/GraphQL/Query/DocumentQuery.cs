using TrackHub.Manager.Application.Documents.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<IReadOnlyCollection<DocumentVm>> GetDocumentsForOwner([Service] ISender sender, [AsParameters] GetDocumentsForOwnerQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<DocumentVm> GetDocument([Service] ISender sender, [AsParameters] GetDocumentQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<DocumentVersionVm>> GetDocumentVersions([Service] ISender sender, [AsParameters] GetDocumentVersionsQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<DocumentVm?> GetActiveDocumentByCategory([Service] ISender sender, [AsParameters] GetActiveDocumentByCategoryQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<DocumentSignatureVm>> GetDocumentSignatures([Service] ISender sender, [AsParameters] GetDocumentSignaturesQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<DocumentVm>> SearchDocuments([Service] ISender sender, SearchDocumentsQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<DocumentVm>> GetExpiringDocuments([Service] ISender sender, [AsParameters] GetExpiringDocumentsQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<PublicLinkGrantVm>> GetDocumentShares([Service] ISender sender, [AsParameters] GetDocumentSharesQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<DocumentTypeVm>> GetDocumentTypes([Service] ISender sender, [AsParameters] GetDocumentTypesQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
    public async Task<IReadOnlyCollection<TransporterDocumentComplianceVm>> GetTransporterDocumentCompliance([Service] ISender sender, [AsParameters] GetTransporterDocumentComplianceQuery query, CancellationToken cancellationToken) => await sender.Send(query, cancellationToken);
}
