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
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);

    // The account's own document type the fixtures upload under; uploads outside the account's types are refused.
    private static void SeedDocumentType(ApplicationDbContext context, Guid accountId)
    {
        context.DocumentTypes.Add(new TrackHub.Manager.Infrastructure.Entities.DocumentType(accountId, "SOAT", "SOAT", true, true, 365, true, DateTimeOffset.UtcNow));
        context.SaveChanges();
    }

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
    public async Task RegisterUploadedDocumentAsync_CreatesDocumentVersionAndAudit()
    {
        await using var context = NewContext(nameof(RegisterUploadedDocumentAsync_CreatesDocumentVersionAndAudit));
        var accountId = Guid.NewGuid();
        SeedDocumentType(context, accountId);
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(), Mock.Of<IAlertRecorder>());

        var vm = await writer.RegisterUploadedDocumentAsync(Guid.NewGuid(), Dto(accountId), CancellationToken.None);

        Assert.That(vm.Category, Is.EqualTo("SOAT"));
        Assert.That(vm.CurrentVersion, Is.EqualTo(1));
        Assert.That(context.DocumentVersions.Count(v => v.DocumentId == vm.DocumentId), Is.EqualTo(1));
        Assert.That(context.AuditEvents.Any(e => e.Action == "UploadDocument"), Is.True);
    }

    [Test]
    public async Task RegisterUploadedDocumentAsync_IsQuarantined_NoDownloadUrl()
    {
        await using var context = NewContext(nameof(RegisterUploadedDocumentAsync_IsQuarantined_NoDownloadUrl));
        var accountId = Guid.NewGuid();
        SeedDocumentType(context, accountId);
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(), Mock.Of<IAlertRecorder>());
        var documentId = Guid.NewGuid();

        var vm = await writer.RegisterUploadedDocumentAsync(documentId, Dto(accountId, "Uploaded", "Quarantined"), CancellationToken.None);

        Assert.That(vm.DocumentId, Is.EqualTo(documentId));
        Assert.That(vm.Status, Is.EqualTo(DocumentStatuses.Uploaded));
        Assert.That(vm.ScanStatus, Is.EqualTo(DocumentScanStatuses.Quarantined));
        Assert.That(vm.DownloadUrl, Is.Null);
    }

    [Test]
    public async Task RegisterUploadedDocumentAsync_NonPrivilegedOwnerDenied_Throws()
    {
        using var context = NewContext(nameof(RegisterUploadedDocumentAsync_NonPrivilegedOwnerDenied_Throws));
        var accountId = Guid.NewGuid();
        SeedDocumentType(context, accountId);
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(privileged: false, ownerAllowed: false), Mock.Of<IAlertRecorder>());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            writer.RegisterUploadedDocumentAsync(Guid.NewGuid(), Dto(accountId, "Uploaded", "Quarantined"), CancellationToken.None));
    }

    [Test]
    public async Task RegisterUploadedDocumentAsync_PrivilegedButCrossAccountOwner_Throws()
    {
        // AC1: even a privileged principal cannot attach to a registered owner the resolver rejects
        // (e.g. a transporter in another account) — the owner check runs for everyone.
        using var context = NewContext(nameof(RegisterUploadedDocumentAsync_PrivilegedButCrossAccountOwner_Throws));
        var accountId = Guid.NewGuid();
        SeedDocumentType(context, accountId);
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(privileged: true, ownerAllowed: false), Mock.Of<IAlertRecorder>());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            writer.RegisterUploadedDocumentAsync(Guid.NewGuid(), Dto(accountId), CancellationToken.None));
    }

    [Test]
    public async Task MarkDocumentScanResultAsync_Clean_ActivatesUploadedDocument()
    {
        await using var context = NewContext(nameof(MarkDocumentScanResultAsync_Clean_ActivatesUploadedDocument));
        var accountId = Guid.NewGuid();
        SeedDocumentType(context, accountId);
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(), Mock.Of<IAlertRecorder>());
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
        SeedDocumentType(context, accountId);
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(), Mock.Of<IAlertRecorder>());
        var created = await writer.RegisterUploadedDocumentAsync(Guid.NewGuid(), Dto(accountId), CancellationToken.None);

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
        SeedDocumentType(context, accountId);
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(), Mock.Of<IAlertRecorder>());
        var doc = await writer.RegisterUploadedDocumentAsync(Guid.NewGuid(), Dto(accountId), CancellationToken.None);

        var sig = await writer.SignDocumentAsync(
            new DocumentSignatureDto(doc.DocumentId, "User", "u", "Jane Staff", "I accept the terms."),
            CancellationToken.None);

        Assert.That(sig.SignerName, Is.EqualTo("Jane Staff"));
        Assert.That(sig.LegalTextAccepted, Is.EqualTo("I accept the terms."));
        Assert.That(context.DocumentSignatures.Any(s => s.DocumentId == doc.DocumentId), Is.True);
        Assert.That(context.AuditEvents.Any(e => e.Action == "SignDocument"), Is.True);
    }

    [Test]
    public async Task VoidDocumentAsync_SetsVoided()
    {
        await using var context = NewContext(nameof(VoidDocumentAsync_SetsVoided));
        var accountId = Guid.NewGuid();
        SeedDocumentType(context, accountId);
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(), Mock.Of<IAlertRecorder>());
        var doc = await writer.RegisterUploadedDocumentAsync(Guid.NewGuid(), Dto(accountId), CancellationToken.None);

        await writer.VoidDocumentAsync(doc.DocumentId, "superseded", CancellationToken.None);

        Assert.That(context.Documents.Single(d => d.DocumentId == doc.DocumentId).Status, Is.EqualTo(DocumentStatuses.Voided));
    }

    [Test]
    public async Task ConfigureDocumentTypeAsync_UpsertsSingleRow()
    {
        await using var context = NewContext(nameof(ConfigureDocumentTypeAsync_UpsertsSingleRow));
        var accountId = Guid.NewGuid();
        SeedDocumentType(context, accountId);
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(), Mock.Of<IAlertRecorder>());

        await writer.ConfigureDocumentTypeAsync(new DocumentTypeDto(accountId, "SOAT", "SOAT policy", true, true, 365), CancellationToken.None);
        var updated = await writer.ConfigureDocumentTypeAsync(new DocumentTypeDto(accountId, "SOAT", "SOAT (renamed)", false, true, 180), CancellationToken.None);

        Assert.That(updated.DisplayName, Is.EqualTo("SOAT (renamed)"));
        Assert.That(context.DocumentTypes.Count(t => t.AccountId == accountId && t.Category == "SOAT"), Is.EqualTo(1));
    }

    private static DocumentVersionDto Version(Guid documentId, DateTimeOffset? expiresAt = null)
        => new(documentId, "local", $"key-{Guid.NewGuid():N}", "hash-2", 20, "application/pdf", "renewed.pdf", null, expiresAt);

    [Test]
    public async Task ReplaceDocumentVersionAsync_ExpiredDocumentWithoutANewExpiry_StaysExpired()
    {
        await using var context = NewContext(nameof(ReplaceDocumentVersionAsync_ExpiredDocumentWithoutANewExpiry_StaysExpired));
        var accountId = Guid.NewGuid();
        SeedDocumentType(context, accountId);
        var alerts = new Mock<IAlertRecorder>();
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(), alerts.Object);
        var documentId = Guid.NewGuid();
        await writer.RegisterUploadedDocumentAsync(documentId, Dto(accountId), CancellationToken.None);
        await writer.ExpireDocumentAsync(documentId, DateTimeOffset.UtcNow.AddDays(-1), CancellationToken.None);

        var vm = await writer.ReplaceDocumentVersionAsync(documentId, Version(documentId), CancellationToken.None);

        Assert.That(vm.Status, Is.EqualTo(DocumentStatuses.Expired));
        alerts.Verify(a => a.ResolveOpenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ReplaceDocumentVersionAsync_WithAFutureExpiry_ReactivatesAndResolvesTheExpiryAlerts()
    {
        await using var context = NewContext(nameof(ReplaceDocumentVersionAsync_WithAFutureExpiry_ReactivatesAndResolvesTheExpiryAlerts));
        var accountId = Guid.NewGuid();
        SeedDocumentType(context, accountId);
        var alerts = new Mock<IAlertRecorder>();
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(), alerts.Object);
        var documentId = Guid.NewGuid();
        await writer.RegisterUploadedDocumentAsync(documentId, Dto(accountId), CancellationToken.None);
        await writer.ExpireDocumentAsync(documentId, DateTimeOffset.UtcNow.AddDays(-1), CancellationToken.None);
        var renewedUntil = DateTimeOffset.UtcNow.AddYears(1);

        var vm = await writer.ReplaceDocumentVersionAsync(documentId, Version(documentId, renewedUntil), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Status, Is.EqualTo(DocumentStatuses.Active));
            Assert.That(vm.ExpiresAt, Is.EqualTo(renewedUntil));
        });
        alerts.Verify(a => a.ResolveOpenAsync(accountId, "Document", documentId.ToString(),
            It.Is<IReadOnlyCollection<string>>(t => t.Contains(AlertEventTypes.DocumentExpiring) && t.Contains(AlertEventTypes.DocumentExpired)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RegisterUploadedDocumentAsync_NormalisesTheClassification()
    {
        await using var context = NewContext(nameof(RegisterUploadedDocumentAsync_NormalisesTheClassification));
        var accountId = Guid.NewGuid();
        SeedDocumentType(context, accountId);
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(), Mock.Of<IAlertRecorder>());

        var vm = await writer.RegisterUploadedDocumentAsync(Guid.NewGuid(), Dto(accountId) with { Classification = " confidential " }, CancellationToken.None);

        Assert.That(vm.Classification, Is.EqualTo(DocumentClassifications.Confidential));
    }

    [Test]
    public async Task RegisterUploadedDocumentAsync_RefusesACategoryOutsideTheAccountsTypes()
    {
        await using var context = NewContext(nameof(RegisterUploadedDocumentAsync_RefusesACategoryOutsideTheAccountsTypes));
        var accountId = Guid.NewGuid();
        var writer = new DocumentWriter(context, Principal(accountId), PrivilegedPolicy(), Mock.Of<IAlertRecorder>());

        await Assert.ThrowsAsync<Common.Application.Exceptions.ValidationException>(() =>
            writer.RegisterUploadedDocumentAsync(Guid.NewGuid(), Dto(accountId), CancellationToken.None));
    }

    [Test]
    public async Task RegisterUploadedDocumentAsync_AStaffFilesOnlyInternalPlatformEvidence()
    {
        await using var context = NewContext(nameof(RegisterUploadedDocumentAsync_AStaffFilesOnlyInternalPlatformEvidence));
        var accountId = Guid.NewGuid();
        SeedDocumentType(context, accountId);
        var writer = new DocumentWriter(context, Principal(accountId, PrincipalType.Staff), PrivilegedPolicy(), Mock.Of<IAlertRecorder>());
        var evidence = Dto(accountId) with { Category = Common.Domain.Evidence.PlatformDocumentCategories.Pod };

        var accepted = await writer.RegisterUploadedDocumentAsync(Guid.NewGuid(), evidence, CancellationToken.None);

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(accepted.Category, Is.EqualTo(Common.Domain.Evidence.PlatformDocumentCategories.Pod));
            await Assert.ThrowsAsync<Common.Application.Exceptions.ValidationException>(() =>
                writer.RegisterUploadedDocumentAsync(Guid.NewGuid(), Dto(accountId), CancellationToken.None), "a compliance category");
            await Assert.ThrowsAsync<Common.Application.Exceptions.ValidationException>(() =>
                writer.RegisterUploadedDocumentAsync(Guid.NewGuid(), evidence with { Classification = DocumentClassifications.Public }, CancellationToken.None), "a public document");
        });
    }
}
