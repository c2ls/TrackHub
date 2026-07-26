using System.Text.Json;
using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class DriverQualificationWriterTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(x => x.AccountId).Returns(accountId);
        principal.SetupGet(x => x.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        return principal.Object;
    }

    private static Driver SeedDriver(ApplicationDbContext context, Guid accountId, string name = "Ana")
    {
        var driver = new Driver(accountId, name, null, null, null, true, null, null, null, null);
        context.Drivers.Add(driver);
        context.SaveChanges();
        return driver;
    }

    private static DriverQualificationDto Dto(Guid accountId, Guid driverId, Guid? documentId = null)
        => new(accountId, driverId, DriverQualificationTypes.License, "C2", "LIC-1",
            new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1), "RUNT", DriverQualificationStatuses.Valid, documentId, null);

    [Test]
    public async Task Create_PersistsQualification_AndRaisesAuditEvent()
    {
        using var context = NewContext(nameof(Create_PersistsQualification_AndRaisesAuditEvent));
        var accountId = Guid.NewGuid();
        var driver = SeedDriver(context, accountId);
        var writer = new DriverQualificationWriter(context, Principal(accountId));

        var vm = await writer.CreateDriverQualificationAsync(Dto(accountId, driver.DriverId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(vm.DriverId, Is.EqualTo(driver.DriverId));
            // The denormalized driver name is filled from the ownership check, not a second query.
            Assert.That(vm.DriverName, Is.EqualTo("Ana"));
            Assert.That(context.DriverQualifications.Count(), Is.EqualTo(1));
            Assert.That(context.AuditEvents.Count(x => x.Action == "CreateDriverQualification"), Is.EqualTo(1));
        });
    }

    [Test]
    public void Create_ForDriverInAnotherAccount_IsNotFound()
    {
        using var context = NewContext(nameof(Create_ForDriverInAnotherAccount_IsNotFound));
        var accountId = Guid.NewGuid();
        var foreignDriver = SeedDriver(context, Guid.NewGuid(), "Foreign");
        var writer = new DriverQualificationWriter(context, Principal(accountId));

        // Cross-account references surface as 404, never as a permission hint (AC1).
        Assert.ThrowsAsync<NotFoundException>(() =>
            writer.CreateDriverQualificationAsync(Dto(accountId, foreignDriver.DriverId), CancellationToken.None));
    }

    [Test]
    public void Create_WithForeignDocumentLink_IsNotFound()
    {
        using var context = NewContext(nameof(Create_WithForeignDocumentLink_IsNotFound));
        var accountId = Guid.NewGuid();
        var driver = SeedDriver(context, accountId);
        var foreignDocument = new Document(Guid.NewGuid(), "Driver", driver.DriverId.ToString(), "User", Guid.NewGuid().ToString(),
            "LocalFileSystem", "key", "application/pdf", 10, "hash", "Internal", "Active", null, "Account", "Clean", "a.pdf", "License");
        context.Documents.Add(foreignDocument);
        context.SaveChanges();

        var writer = new DriverQualificationWriter(context, Principal(accountId));

        Assert.ThrowsAsync<NotFoundException>(() =>
            writer.CreateDriverQualificationAsync(Dto(accountId, driver.DriverId, foreignDocument.DocumentId), CancellationToken.None));
    }

    [Test]
    public async Task Delete_RemovesRow_AndKeepsBeforeImageInAudit()
    {
        using var context = NewContext(nameof(Delete_RemovesRow_AndKeepsBeforeImageInAudit));
        var accountId = Guid.NewGuid();
        var driver = SeedDriver(context, accountId);
        var writer = new DriverQualificationWriter(context, Principal(accountId));
        var created = await writer.CreateDriverQualificationAsync(Dto(accountId, driver.DriverId), CancellationToken.None);

        await writer.DeleteDriverQualificationAsync(created.DriverQualificationId, CancellationToken.None);

        var audit = context.AuditEvents.Single(x => x.Action == "DeleteDriverQualification");
        Assert.Multiple(() =>
        {
            Assert.That(context.DriverQualifications.Count(), Is.EqualTo(0));
            // The hard delete is only safe because the pre-delete state survives in the audit trail.
            Assert.That(audit.OldValuesJson, Does.Contain("LIC-1"));
            Assert.That(audit.NewValuesJson, Is.Null);
        });
    }

    // The audit payloads are hand-built JSON. Free-text fields (Notes is 500 chars) can legally contain
    // newlines, quotes and backslashes — all of which must survive as valid, round-trippable JSON.
    [Test]
    public async Task AuditPayload_IsValidJson_ForFreeTextContainingControlCharacters()
    {
        using var context = NewContext(nameof(AuditPayload_IsValidJson_ForFreeTextContainingControlCharacters));
        var accountId = Guid.NewGuid();
        var driver = SeedDriver(context, accountId);
        var writer = new DriverQualificationWriter(context, Principal(accountId));

        var awkward = "Renewed at RUNT\nPending \"photo\"\tand a backslash \\ plus ñ";
        var dto = Dto(accountId, driver.DriverId) with { Notes = awkward, IssuingAuthority = "Line1\r\nLine2" };
        await writer.CreateDriverQualificationAsync(dto, CancellationToken.None);

        var audit = context.AuditEvents.Single(x => x.Action == "CreateDriverQualification");
        using var parsed = JsonDocument.Parse(audit.NewValuesJson!);
        Assert.That(parsed.RootElement.GetProperty("issuingAuthority").GetString(), Is.EqualTo("Line1\r\nLine2"));
    }

    [Test]
    public void Update_AcrossAccounts_IsForbidden()
    {
        using var context = NewContext(nameof(Update_AcrossAccounts_IsForbidden));
        var accountId = Guid.NewGuid();
        var driver = SeedDriver(context, accountId);
        var writer = new DriverQualificationWriter(context, Principal(accountId));
        var created = writer.CreateDriverQualificationAsync(Dto(accountId, driver.DriverId), CancellationToken.None).Result;

        var moved = Dto(Guid.NewGuid(), driver.DriverId);
        Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            writer.UpdateDriverQualificationAsync(created.DriverQualificationId, moved, CancellationToken.None));
    }
}
