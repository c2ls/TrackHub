using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class DocumentWriterTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid accountId, PrincipalType type = PrincipalType.User)
    {
        var p = new Mock<ICurrentPrincipal>();
        p.SetupGet(x => x.AccountId).Returns(accountId);
        p.SetupGet(x => x.PrincipalType).Returns(type);
        p.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        return p.Object;
    }

    // Privileged policy bypasses owner-visibility so writer logic is tested in isolation.
    private static IDocumentAccessPolicy PrivilegedPolicy(bool privileged = true, bool ownerAllowed = true)
    {
        var policy = new Mock<IDocumentAccessPolicy>();
        policy.SetupGet(p => p.IsPrivilegedPrincipal).Returns(privileged);
        policy.Setup(p => p.CanAccessOwnerAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerAllowed);
        return policy.Object;
    }

    private static DocumentDto Dto(Guid accountId, string status = "Active", string scan = "Clean")
        => new(accountId, DocumentOwnerTypes.Transporter, Guid.NewGuid().ToString(), "User", "u", "local", "key", "application/pdf", 10, "hash", DocumentClassifications.Internal, status, null, "Owner", scan, "soat.pdf", "SOAT");

    [Test]
    public async Task CreateDocumentMetadataAsync_CreatesDocumentVersionAndAudit()
    {
        await using var context = NewContext(nameof(CreateDocumentMetadataAsync_CreatesDocumentVersionAndAudit));
        var accountId = Guid.NewGuid();
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy());

        var vm = await writer.CreateDocumentMetadataAsync(Dto(accountId), CancellationToken.None);

        Assert.That(vm.Category, Is.EqualTo("SOAT"));
        Assert.That(vm.CurrentVersion, Is.EqualTo(1));
        Assert.That(context.DocumentVersions.Count(v => v.DocumentId == vm.DocumentId), Is.EqualTo(1));
        Assert.That(context.AuditEvents.Any(e => e.Action == "CreateDocument"), Is.True);
    }

    [Test]
    public async Task RegisterUploadedDocumentAsync_IsQuarantined_NoDownloadUrl()
    {
        await using var context = NewContext(nameof(RegisterUploadedDocumentAsync_IsQuarantined_NoDownloadUrl));
        var accountId = Guid.NewGuid();
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy());
        var documentId = Guid.NewGuid();

        var vm = await writer.RegisterUploadedDocumentAsync(documentId, Dto(accountId, "Uploaded", "Quarantined"), CancellationToken.None);

        Assert.That(vm.DocumentId, Is.EqualTo(documentId));
        Assert.That(vm.Status, Is.EqualTo(DocumentStatuses.Uploaded));
        Assert.That(vm.ScanStatus, Is.EqualTo(DocumentScanStatuses.Quarantined));
        Assert.That(vm.DownloadUrl, Is.Null);
    }

    [Test]
    public void RegisterUploadedDocumentAsync_NonPrivilegedOwnerDenied_Throws()
    {
        using var context = NewContext(nameof(RegisterUploadedDocumentAsync_NonPrivilegedOwnerDenied_Throws));
        var accountId = Guid.NewGuid();
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(privileged: false, ownerAllowed: false));

        Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            writer.RegisterUploadedDocumentAsync(Guid.NewGuid(), Dto(accountId, "Uploaded", "Quarantined"), CancellationToken.None));
    }

    [Test]
    public void CreateDocumentMetadataAsync_PrivilegedButCrossAccountOwner_Throws()
    {
        // AC1: even a privileged principal cannot attach to a registered owner the resolver rejects
        // (e.g. a transporter in another account) — the owner check runs for everyone.
        using var context = NewContext(nameof(CreateDocumentMetadataAsync_PrivilegedButCrossAccountOwner_Throws));
        var accountId = Guid.NewGuid();
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(privileged: true, ownerAllowed: false));

        Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            writer.CreateDocumentMetadataAsync(Dto(accountId), CancellationToken.None));
    }

    [Test]
    public async Task MarkDocumentScanResultAsync_Clean_ActivatesUploadedDocument()
    {
        await using var context = NewContext(nameof(MarkDocumentScanResultAsync_Clean_ActivatesUploadedDocument));
        var accountId = Guid.NewGuid();
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy());
        var id = Guid.NewGuid();
        await writer.RegisterUploadedDocumentAsync(id, Dto(accountId, "Uploaded", "Quarantined"), CancellationToken.None);

        await writer.MarkDocumentScanResultAsync(id, DocumentScanStatuses.Clean, CancellationToken.None);

        var doc = context.Documents.Single(d => d.DocumentId == id);
        Assert.That(doc.Status, Is.EqualTo(DocumentStatuses.Active));
        Assert.That(doc.ScanStatus, Is.EqualTo(DocumentScanStatuses.Clean));
    }

    [Test]
    public async Task ReplaceDocumentVersionAsync_IncrementsVersion_AndReQuarantines()
    {
        await using var context = NewContext(nameof(ReplaceDocumentVersionAsync_IncrementsVersion_AndReQuarantines));
        var accountId = Guid.NewGuid();
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy());
        var created = await writer.CreateDocumentMetadataAsync(Dto(accountId), CancellationToken.None);

        var vm = await writer.ReplaceDocumentVersionAsync(created.DocumentId,
            new DocumentVersionDto(created.DocumentId, "local", "key2", "hash2", 20, "application/pdf", "soat-v2.pdf", "renewal"),
            CancellationToken.None);

        Assert.That(vm.CurrentVersion, Is.EqualTo(2));
        Assert.That(vm.ScanStatus, Is.EqualTo(DocumentScanStatuses.Quarantined));
        Assert.That(context.DocumentVersions.Count(v => v.DocumentId == created.DocumentId), Is.EqualTo(2));
    }

    [Test]
    public async Task SignDocumentAsync_RecordsSignatureAndAudit()
    {
        await using var context = NewContext(nameof(SignDocumentAsync_RecordsSignatureAndAudit));
        var accountId = Guid.NewGuid();
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy());
        var doc = await writer.CreateDocumentMetadataAsync(Dto(accountId), CancellationToken.None);

        var sig = await writer.SignDocumentAsync(
            new DocumentSignatureDto(doc.DocumentId, "User", "u", "Jane Driver", "I accept the terms."),
            CancellationToken.None);

        Assert.That(sig.SignerName, Is.EqualTo("Jane Driver"));
        Assert.That(sig.LegalTextAccepted, Is.EqualTo("I accept the terms."));
        Assert.That(context.DocumentSignatures.Any(s => s.DocumentId == doc.DocumentId), Is.True);
        Assert.That(context.AuditEvents.Any(e => e.Action == "SignDocument"), Is.True);
    }

    [Test]
    public async Task VoidDocumentAsync_SetsVoided()
    {
        await using var context = NewContext(nameof(VoidDocumentAsync_SetsVoided));
        var accountId = Guid.NewGuid();
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy());
        var doc = await writer.CreateDocumentMetadataAsync(Dto(accountId), CancellationToken.None);

        await writer.VoidDocumentAsync(doc.DocumentId, "superseded", CancellationToken.None);

        Assert.That(context.Documents.Single(d => d.DocumentId == doc.DocumentId).Status, Is.EqualTo(DocumentStatuses.Voided));
    }

    [Test]
    public async Task ConfigureDocumentTypeAsync_UpsertsSingleRow()
    {
        await using var context = NewContext(nameof(ConfigureDocumentTypeAsync_UpsertsSingleRow));
        var accountId = Guid.NewGuid();
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy());

        await writer.ConfigureDocumentTypeAsync(new DocumentTypeDto(accountId, "SOAT", "SOAT policy", true, true, 365), CancellationToken.None);
        var updated = await writer.ConfigureDocumentTypeAsync(new DocumentTypeDto(accountId, "SOAT", "SOAT (renamed)", false, true, 180), CancellationToken.None);

        Assert.That(updated.DisplayName, Is.EqualTo("SOAT (renamed)"));
        Assert.That(context.DocumentTypes.Count(t => t.AccountId == accountId && t.Category == "SOAT"), Is.EqualTo(1));
    }
}
