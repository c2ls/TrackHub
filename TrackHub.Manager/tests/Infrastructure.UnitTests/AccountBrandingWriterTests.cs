using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class AccountBrandingWriterTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(p => p.UserId).Returns(Guid.NewGuid());
        return principal.Object;
    }

    private static async Task<Guid> SeedAccountAsync(ApplicationDbContext context)
    {
        var account = new Account("Acme", "Description", 2, true);
        await context.Accounts.AddAsync(account);
        await context.SaveChangesAsync(CancellationToken.None);
        return account.AccountId;
    }

    [Test]
    public async Task UpsertBrandingAsync_InsertsThenUpdates_SameRow()
    {
        await using var context = NewContext(nameof(UpsertBrandingAsync_InsertsThenUpdates_SameRow));
        var accountId = await SeedAccountAsync(context);
        var writer = new AccountBrandingWriter(context, Principal(accountId));

        var inserted = await writer.UpsertBrandingAsync(
            new AccountBrandingDto(accountId, "Acme Fleet", null, "#1A73E8", "Header"), CancellationToken.None);
        Assert.That(inserted.DisplayName, Is.EqualTo("Acme Fleet"));

        var updated = await writer.UpsertBrandingAsync(
            new AccountBrandingDto(accountId, "Acme Logistics", null, "#FF0000", null), CancellationToken.None);

        Assert.That(updated.DisplayName, Is.EqualTo("Acme Logistics"));
        Assert.That(updated.PrimaryColor, Is.EqualTo("#FF0000"));
        Assert.That(context.AccountBrandings.Count(b => b.AccountId == accountId), Is.EqualTo(1));
    }

    [Test]
    public async Task UpsertBrandingAsync_WritesBrandingChangedAudit()
    {
        await using var context = NewContext(nameof(UpsertBrandingAsync_WritesBrandingChangedAudit));
        var accountId = await SeedAccountAsync(context);
        var writer = new AccountBrandingWriter(context, Principal(accountId));

        await writer.UpsertBrandingAsync(
            new AccountBrandingDto(accountId, "Acme Fleet", null, "#1A73E8", null), CancellationToken.None);

        var audit = context.AuditEvents.SingleOrDefault(e => e.Action == "BrandingChanged" && e.AccountId == accountId);
        Assert.That(audit, Is.Not.Null);
    }

    [Test]
    public async Task GetBrandingAsync_NoRow_ReturnsPlatformDefault()
    {
        await using var context = NewContext(nameof(GetBrandingAsync_NoRow_ReturnsPlatformDefault));
        var accountId = await SeedAccountAsync(context);
        var reader = new AccountBrandingReader(context, Principal(accountId));

        var branding = await reader.GetBrandingAsync(accountId, CancellationToken.None);

        Assert.That(branding.AccountId, Is.EqualTo(accountId));
        Assert.That(branding.PrimaryColor, Is.EqualTo(AccountBrandingReader.DefaultPrimaryColor));
        Assert.That(branding.DisplayName, Is.EqualTo("Acme")); // defaults to the account name
    }

    [Test]
    public async Task LogoDocumentBelongsToAccountAsync_MatchesOwnerAndAccount()
    {
        await using var context = NewContext(nameof(LogoDocumentBelongsToAccountAsync_MatchesOwnerAndAccount));
        var accountId = await SeedAccountAsync(context);
        var documentId = Guid.NewGuid();
        var doc = new Document(accountId, AccountBrandingReader.BrandingDocumentOwnerType, accountId.ToString(),
            "User", "u", "local", "key", "image/png", 10, "hash", "internal", "Pending", null, "account", "clean");
        // Force the generated DocumentId to the known value via reflection-free path: query by owner instead.
        await context.Documents.AddAsync(doc);
        await context.SaveChangesAsync(CancellationToken.None);
        var reader = new AccountBrandingReader(context, Principal(accountId));

        var ok = await reader.LogoDocumentBelongsToAccountAsync(accountId, doc.DocumentId, CancellationToken.None);
        var wrongOwnerAccount = await reader.LogoDocumentBelongsToAccountAsync(Guid.NewGuid(), doc.DocumentId, CancellationToken.None);

        Assert.That(ok, Is.True);
        Assert.That(wrongOwnerAccount, Is.False);
    }
}
