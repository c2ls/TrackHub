using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Application.Documents.Commands;

// Low-level metadata primitive stays ungated so embedded module panels keep working.
[Authorize(Resource = Resources.Documents, Action = Actions.Write)]
public readonly record struct CreateDocumentMetadataCommand(DocumentDto Document) : IRequest<DocumentVm>;
public class CreateDocumentMetadataCommandHandler(IDocumentWriter writer) : IRequestHandler<CreateDocumentMetadataCommand, DocumentVm>
{
    public async Task<DocumentVm> Handle(CreateDocumentMetadataCommand request, CancellationToken cancellationToken) => await writer.CreateDocumentMetadataAsync(request.Document, cancellationToken);
}
public class CreateDocumentMetadataCommandValidator : AbstractValidator<CreateDocumentMetadataCommand>
{
    public CreateDocumentMetadataCommandValidator()
    {
        RuleFor(x => x.Document.OwnerEntityType).NotEmpty();
        RuleFor(x => x.Document.OwnerEntityId).NotEmpty();
        RuleFor(x => x.Document.FileName).NotEmpty();
        RuleFor(x => x.Document.Category).NotEmpty();
        RuleFor(x => x.Document.Classification).Must(DocumentClassifications.IsValid).WithMessage("Invalid document classification.");
        RuleFor(x => x.Document.Status).Must(DocumentStatuses.IsValid).WithMessage("Invalid document status.");
        RuleFor(x => x.Document.ScanStatus).Must(DocumentScanStatuses.IsValid).WithMessage("Invalid scan status.");
    }
}

// Completes the upload endpoint: bytes already streamed to storage under a
// server-generated key. Allowed for User and Driver principals; ungated (embedded module panels).
[Authorize(Resource = Resources.Documents, Action = Actions.Write, PrincipalTypes = "User,Driver")]
public readonly record struct RegisterUploadedDocumentCommand(Guid DocumentId, DocumentDto Document) : IRequest<DocumentVm>;
public class RegisterUploadedDocumentCommandHandler(IDocumentWriter writer) : IRequestHandler<RegisterUploadedDocumentCommand, DocumentVm>
{
    public async Task<DocumentVm> Handle(RegisterUploadedDocumentCommand request, CancellationToken cancellationToken) => await writer.RegisterUploadedDocumentAsync(request.DocumentId, request.Document, cancellationToken);
}

// Upload-completion bookkeeping driven by the ingestion pipeline, not a user-editable field.
// ServiceClient-only, so the register → scan → activate sequence that the upload endpoint and
// DocumentScanService implement remains the only way a document's lifecycle status advances.
[Authorize(Resource = Resources.Documents, Action = Actions.Edit, PrincipalTypes = "ServiceClient")]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct MarkDocumentUploadedCommand(Guid DocumentId, string Status) : IRequest;
public class MarkDocumentUploadedCommandHandler(IDocumentWriter writer) : IRequestHandler<MarkDocumentUploadedCommand>
{
    public async Task Handle(MarkDocumentUploadedCommand request, CancellationToken cancellationToken) => await writer.MarkDocumentUploadedAsync(request.DocumentId, request.Status, cancellationToken);
}
public class MarkDocumentUploadedCommandValidator : AbstractValidator<MarkDocumentUploadedCommand>
{
    public MarkDocumentUploadedCommandValidator() => RuleFor(x => x.Status).Must(DocumentStatuses.IsValid).WithMessage("Invalid document status.");
}

// Called by the scan-result processing job under a scoped service identity.
//
// ServiceClient-only, and this is a SECURITY boundary rather than bookkeeping: the malware verdict is
// what stands between an infected upload and the download endpoints, which serve bytes only for a
// Clean scan status. DocumentScanService re-scans rows still marked Quarantined, so a verdict written
// here is final until the version is replaced.
[Authorize(Resource = Resources.Documents, Action = Actions.Edit, PrincipalTypes = "ServiceClient")]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct MarkDocumentScanResultCommand(Guid DocumentId, string ScanStatus) : IRequest;
public class MarkDocumentScanResultCommandHandler(IDocumentWriter writer) : IRequestHandler<MarkDocumentScanResultCommand>
{
    public async Task Handle(MarkDocumentScanResultCommand request, CancellationToken cancellationToken) => await writer.MarkDocumentScanResultAsync(request.DocumentId, request.ScanStatus, cancellationToken);
}
public class MarkDocumentScanResultCommandValidator : AbstractValidator<MarkDocumentScanResultCommand>
{
    public MarkDocumentScanResultCommandValidator() => RuleFor(x => x.ScanStatus).Must(DocumentScanStatuses.IsValid).WithMessage("Invalid scan status.");
}

[Authorize(Resource = Resources.Documents, Action = Actions.Edit)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct ExpireDocumentCommand(Guid DocumentId, DateTimeOffset ExpiresAt) : IRequest;
public class ExpireDocumentCommandHandler(IDocumentWriter writer) : IRequestHandler<ExpireDocumentCommand>
{
    public async Task Handle(ExpireDocumentCommand request, CancellationToken cancellationToken) => await writer.ExpireDocumentAsync(request.DocumentId, request.ExpiresAt, cancellationToken);
}

[Authorize(Resource = Resources.Documents, Action = Actions.Delete)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct DeleteDocumentReferenceCommand(Guid DocumentId) : IRequest;
public class DeleteDocumentReferenceCommandHandler(IDocumentWriter writer) : IRequestHandler<DeleteDocumentReferenceCommand>
{
    public async Task Handle(DeleteDocumentReferenceCommand request, CancellationToken cancellationToken) => await writer.DeleteDocumentReferenceAsync(request.DocumentId, cancellationToken);
}

// Bytes arrive via the upload endpoint; the command completes the version re-point.
[Authorize(Resource = Resources.Documents, Action = Actions.Edit)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct ReplaceDocumentVersionCommand(Guid DocumentId, DocumentVersionDto NewVersion) : IRequest<DocumentVm>;
public class ReplaceDocumentVersionCommandHandler(IDocumentWriter writer) : IRequestHandler<ReplaceDocumentVersionCommand, DocumentVm>
{
    public async Task<DocumentVm> Handle(ReplaceDocumentVersionCommand request, CancellationToken cancellationToken) => await writer.ReplaceDocumentVersionAsync(request.DocumentId, request.NewVersion, cancellationToken);
}
public class ReplaceDocumentVersionCommandValidator : AbstractValidator<ReplaceDocumentVersionCommand>
{
    public ReplaceDocumentVersionCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.NewVersion.FileName).NotEmpty();
        RuleFor(x => x.NewVersion.StorageKey).NotEmpty();
        RuleFor(x => x.NewVersion.Sha256Hash).NotEmpty();
    }
}

[Authorize(Resource = Resources.Documents, Action = Actions.Edit)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct VoidDocumentCommand(Guid DocumentId, string Reason) : IRequest;
public class VoidDocumentCommandHandler(IDocumentWriter writer) : IRequestHandler<VoidDocumentCommand>
{
    public async Task Handle(VoidDocumentCommand request, CancellationToken cancellationToken) => await writer.VoidDocumentAsync(request.DocumentId, request.Reason, cancellationToken);
}
public class VoidDocumentCommandValidator : AbstractValidator<VoidDocumentCommand>
{
    public VoidDocumentCommandValidator() => RuleFor(x => x.Reason).NotEmpty();
}

[Authorize(Resource = Resources.Documents, Action = Actions.Edit)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct SignDocumentCommand(DocumentSignatureDto Signature) : IRequest<DocumentSignatureVm>;
public class SignDocumentCommandHandler(IDocumentWriter writer) : IRequestHandler<SignDocumentCommand, DocumentSignatureVm>
{
    public async Task<DocumentSignatureVm> Handle(SignDocumentCommand request, CancellationToken cancellationToken) => await writer.SignDocumentAsync(request.Signature, cancellationToken);
}
public class SignDocumentCommandValidator : AbstractValidator<SignDocumentCommand>
{
    public SignDocumentCommandValidator()
    {
        RuleFor(x => x.Signature.DocumentId).NotEmpty();
        RuleFor(x => x.Signature.SignerName).NotEmpty();
        RuleFor(x => x.Signature.LegalTextAccepted).NotEmpty();
    }
}

// Standalone document-type configuration is gated by the `documents` feature.
[Authorize(Resource = Resources.Documents, Action = Actions.Write)]
[RequireFeature(FeatureKeys.Documents)]
public readonly record struct ConfigureDocumentTypeCommand(DocumentTypeDto DocumentType) : IRequest<DocumentTypeVm>;
public class ConfigureDocumentTypeCommandHandler(IDocumentWriter writer) : IRequestHandler<ConfigureDocumentTypeCommand, DocumentTypeVm>
{
    public async Task<DocumentTypeVm> Handle(ConfigureDocumentTypeCommand request, CancellationToken cancellationToken) => await writer.ConfigureDocumentTypeAsync(request.DocumentType, cancellationToken);
}
public class ConfigureDocumentTypeCommandValidator : AbstractValidator<ConfigureDocumentTypeCommand>
{
    public ConfigureDocumentTypeCommandValidator() => RuleFor(x => x.DocumentType.Category).NotEmpty();
}

[Authorize(Resource = Resources.Documents, Action = Actions.Write)]
[RequireFeature(FeatureKeys.Documents)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct DisableDocumentTypeCommand(Guid DocumentTypeId) : IRequest;
public class DisableDocumentTypeCommandHandler(IDocumentWriter writer) : IRequestHandler<DisableDocumentTypeCommand>
{
    public async Task Handle(DisableDocumentTypeCommand request, CancellationToken cancellationToken) => await writer.DisableDocumentTypeAsync(request.DocumentTypeId, cancellationToken);
}
