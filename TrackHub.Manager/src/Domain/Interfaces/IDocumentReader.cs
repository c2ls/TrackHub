namespace TrackHub.Manager.Domain.Interfaces;

public interface IDocumentReader
{
    Task<IReadOnlyCollection<DocumentVm>> GetDocumentsForOwnerAsync(Guid accountId, string ownerEntityType, string ownerEntityId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken);
    Task<DocumentVm> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DocumentVersionVm>> GetDocumentVersionsAsync(Guid documentId, int skip, int take, CancellationToken cancellationToken);
    Task<DocumentVm?> GetActiveDocumentByCategoryAsync(string ownerEntityType, string ownerEntityId, string category, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DocumentVm>> SearchDocumentsAsync(DocumentSearchFilter filter, int skip, int take, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DocumentVm>> GetExpiringDocumentsAsync(int withinDays, int skip, int take, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PublicLinkGrantVm>> GetDocumentSharesAsync(Guid documentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DocumentTypeVm>> GetDocumentTypesAsync(Guid accountId, bool includeDisabled, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DocumentSignatureVm>> GetDocumentSignaturesAsync(Guid documentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TransporterDocumentComplianceVm>> GetTransporterDocumentComplianceAsync(Guid accountId, CancellationToken cancellationToken);
}
