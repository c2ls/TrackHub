namespace TrackHub.Manager.Domain.Interfaces;

public interface IDocumentWriter
{
    Task<DocumentVm> CreateDocumentMetadataAsync(DocumentDto document, CancellationToken cancellationToken);

    /// <summary>
    /// Registers a document whose bytes have just been streamed to storage under a server-generated key
    ///. Creates the Document (Status = Uploaded, ScanStatus = Quarantined) plus
    /// version 1, using the caller-supplied <paramref name="documentId"/> that names the storage key.
    /// </summary>
    Task<DocumentVm> RegisterUploadedDocumentAsync(Guid documentId, DocumentDto document, CancellationToken cancellationToken);

    Task MarkDocumentUploadedAsync(Guid documentId, string status, CancellationToken cancellationToken);
    Task MarkDocumentScanResultAsync(Guid documentId, string scanStatus, CancellationToken cancellationToken);
    Task ExpireDocumentAsync(Guid documentId, DateTimeOffset expiresAt, CancellationToken cancellationToken);
    Task DeleteDocumentReferenceAsync(Guid documentId, CancellationToken cancellationToken);

    Task<DocumentVm> ReplaceDocumentVersionAsync(Guid documentId, DocumentVersionDto newVersion, CancellationToken cancellationToken);
    Task VoidDocumentAsync(Guid documentId, string reason, CancellationToken cancellationToken);
    Task<DocumentSignatureVm> SignDocumentAsync(DocumentSignatureDto signature, CancellationToken cancellationToken);

    Task<DocumentTypeVm> ConfigureDocumentTypeAsync(DocumentTypeDto documentType, CancellationToken cancellationToken);
    Task DisableDocumentTypeAsync(Guid documentTypeId, CancellationToken cancellationToken);
}
