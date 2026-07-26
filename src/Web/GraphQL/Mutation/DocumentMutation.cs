using TrackHub.Manager.Application.Documents.Commands;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<DocumentVm> CreateDocumentMetadata([Service] ISender sender, CreateDocumentMetadataCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> MarkDocumentUploaded([Service] ISender sender, MarkDocumentUploadedCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> MarkDocumentScanResult([Service] ISender sender, MarkDocumentScanResultCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> ExpireDocument([Service] ISender sender, ExpireDocumentCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<Guid> DeleteDocumentReference([Service] ISender sender, DeleteDocumentReferenceCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return command.DocumentId; }
    public async Task<DocumentVm> ReplaceDocumentVersion([Service] ISender sender, ReplaceDocumentVersionCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> VoidDocument([Service] ISender sender, VoidDocumentCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<DocumentSignatureVm> SignDocument([Service] ISender sender, SignDocumentCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<DocumentTypeVm> ConfigureDocumentType([Service] ISender sender, ConfigureDocumentTypeCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<Guid> DisableDocumentType([Service] ISender sender, DisableDocumentTypeCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return command.DocumentTypeId; }
}
