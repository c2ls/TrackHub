using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class DocumentReaderTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var p = new Mock<ICurrentPrincipal>();
        p.SetupGet(x => x.AccountId).Returns(accountId);
        p.SetupGet(x => x.PrincipalType).Returns(PrincipalType.User);
        p.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        return p.Object;
    }

    private static Mock<IDocumentAccessPolicy> Policy(bool ownerAllowed = true, bool clearedForSensitive = true, bool privileged = false)
    {
        var policy = new Mock<IDocumentAccessPolicy>();
        policy.SetupGet(p => p.IsPrivilegedPrincipal).Returns(privileged);
        policy.Setup(p => p.CanAccessOwnerAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ownerAllowed);
        policy.Setup(p => p.IsClearedForClassification(It.IsAny<string>()))
            .Returns<string>(c => !DocumentClassifications.IsSensitive(c) || clearedForSensitive);
        return policy;
    }

    private static Document Doc(Guid accountId, string ownerId, string category, string classification, string status = "Active", string scan = "Clean")
        => new(accountId, DocumentOwnerTypes.Transporter, ownerId, "User", "u", "local", "key", "application/pdf", 10, "hash", classification, status, null, "Owner", scan, "f.pdf", category);

    [Test]
    public void GetDocumentsForOwnerAsync_OwnerNotVisible_Throws()
    {
        using var context = NewContext(nameof(GetDocumentsForOwnerAsync_OwnerNotVisible_Throws));
        var accountId = Guid.NewGuid();
        var reader = new DocumentReader(context, Principal(accountId), Policy(ownerAllowed: false).Object);

        Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            reader.GetDocumentsForOwnerAsync(accountId, DocumentOwnerTypes.Transporter, Guid.NewGuid().ToString(), null, null, 0, 50, CancellationToken.None));
    }

    [Test]
    public async Task GetDocumentsForOwnerAsync_FiltersSensitive_WhenNotCleared()
    {
        await using var context = NewContext(nameof(GetDocumentsForOwnerAsync_FiltersSensitive_WhenNotCleared));
        var accountId = Guid.NewGuid();
        var ownerId = Guid.NewGuid().ToString();
        await context.Documents.AddRangeAsync(
            Doc(accountId, ownerId, "POD", DocumentClassifications.Public),
            Doc(accountId, ownerId, "Medical", DocumentClassifications.Confidential));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DocumentReader(context, Principal(accountId), Policy(clearedForSensitive: false).Object);
        var docs = await reader.GetDocumentsForOwnerAsync(accountId, DocumentOwnerTypes.Transporter, ownerId, null, null, 0, 50, CancellationToken.None);

        Assert.That(docs.Count, Is.EqualTo(1));
        Assert.That(docs.Single().Classification, Is.EqualTo(DocumentClassifications.Public));
    }

    [Test]
    public async Task GetDocumentsForOwnerAsync_ClearedForSensitive_ReturnsAll()
    {
        await using var context = NewContext(nameof(GetDocumentsForOwnerAsync_ClearedForSensitive_ReturnsAll));
        var accountId = Guid.NewGuid();
        var ownerId = Guid.NewGuid().ToString();
        await context.Documents.AddRangeAsync(
            Doc(accountId, ownerId, "POD", DocumentClassifications.Public),
            Doc(accountId, ownerId, "Medical", DocumentClassifications.Confidential));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DocumentReader(context, Principal(accountId), Policy(clearedForSensitive: true, privileged: true).Object);
        var docs = await reader.GetDocumentsForOwnerAsync(accountId, DocumentOwnerTypes.Transporter, ownerId, null, null, 0, 50, CancellationToken.None);

        Assert.That(docs.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetDocumentAsync_Clean_PopulatesDownloadUrl()
    {
        await using var context = NewContext(nameof(GetDocumentAsync_Clean_PopulatesDownloadUrl));
        var accountId = Guid.NewGuid();
        var doc = Doc(accountId, Guid.NewGuid().ToString(), "SOAT", DocumentClassifications.Internal, scan: DocumentScanStatuses.Clean);
        await context.Documents.AddAsync(doc);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DocumentReader(context, Principal(accountId), Policy().Object);
        var vm = await reader.GetDocumentAsync(doc.DocumentId, CancellationToken.None);

        Assert.That(vm.DownloadUrl, Is.Not.Null);
        Assert.That(vm.DownloadUrl, Does.Contain(doc.DocumentId.ToString()));
    }

    [Test]
    public async Task GetDocumentAsync_Quarantined_NoDownloadUrl()
    {
        await using var context = NewContext(nameof(GetDocumentAsync_Quarantined_NoDownloadUrl));
        var accountId = Guid.NewGuid();
        var doc = Doc(accountId, Guid.NewGuid().ToString(), "SOAT", DocumentClassifications.Internal, status: "Uploaded", scan: DocumentScanStatuses.Quarantined);
        await context.Documents.AddAsync(doc);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DocumentReader(context, Principal(accountId), Policy().Object);
        var vm = await reader.GetDocumentAsync(doc.DocumentId, CancellationToken.None);

        Assert.That(vm.DownloadUrl, Is.Null);
    }

    [Test]
    public async Task GetDocumentAsync_SensitiveNotCleared_ThrowsNotFound_NonDisclosing()
    {
        // Non-disclosure: an uncleared Confidential doc must 404 (not 403) so existence isn't confirmed.
        await using var context = NewContext(nameof(GetDocumentAsync_SensitiveNotCleared_ThrowsNotFound_NonDisclosing));
        var accountId = Guid.NewGuid();
        var doc = Doc(accountId, Guid.NewGuid().ToString(), "Medical", DocumentClassifications.Confidential);
        await context.Documents.AddAsync(doc);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DocumentReader(context, Principal(accountId), Policy(clearedForSensitive: false).Object);

        Assert.ThrowsAsync<Ardalis.GuardClauses.NotFoundException>(() =>
            reader.GetDocumentAsync(doc.DocumentId, CancellationToken.None));
    }

    [Test]
    public async Task GetActiveDocumentByCategoryAsync_ReturnsActive()
    {
        await using var context = NewContext(nameof(GetActiveDocumentByCategoryAsync_ReturnsActive));
        var accountId = Guid.NewGuid();
        var ownerId = Guid.NewGuid().ToString();
        await context.Documents.AddAsync(Doc(accountId, ownerId, "SOAT", DocumentClassifications.Internal));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DocumentReader(context, Principal(accountId), Policy().Object);
        var vm = await reader.GetActiveDocumentByCategoryAsync(DocumentOwnerTypes.Transporter, ownerId, "SOAT", CancellationToken.None);

        Assert.That(vm, Is.Not.Null);
        Assert.That(vm!.Value.Category, Is.EqualTo("SOAT"));
    }
}
