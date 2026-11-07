using System.Text.Json;
using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class StaffQualificationWriterTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(x => x.AccountId).Returns(accountId);
        principal.SetupGet(x => x.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        return principal.Object;
    }

    private static StaffMember SeedStaff(ApplicationDbContext context, Guid accountId, string name = "Ana")
    {
        var staff = new StaffMember(accountId, name, null, null, null, true, null, null, null, null);
        context.StaffMembers.Add(staff);
        context.SaveChanges();
        return staff;
    }

    private static StaffQualificationDto Dto(Guid accountId, Guid staffId, Guid? documentId = null)
        => new(accountId, staffId, StaffQualificationTypes.License, "C2", "LIC-1",
            new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1), "RUNT", StaffQualificationStatuses.Valid, documentId, null);

    [Test]
    public async Task Create_PersistsQualification_AndRaisesAuditEvent()
    {
        using var context = NewContext(nameof(Create_PersistsQualification_AndRaisesAuditEvent));
        var accountId = Guid.NewGuid();
        var staff = SeedStaff(context, accountId);
        var writer = new StaffQualificationWriter(context, Principal(accountId), Mock.Of<IAlertRecorder>());

        var vm = await writer.CreateStaffQualificationAsync(Dto(accountId, staff.StaffId), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(vm.StaffId, Is.EqualTo(staff.StaffId));
            // The denormalized staff name is filled from the ownership check, not a second query.
            Assert.That(vm.StaffName, Is.EqualTo("Ana"));
            Assert.That(context.StaffQualifications.Count(), Is.EqualTo(1));
            Assert.That(context.AuditEvents.Count(x => x.Action == "CreateStaffQualification"), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Create_ForStaffInAnotherAccount_IsNotFound()
    {
        using var context = NewContext(nameof(Create_ForStaffInAnotherAccount_IsNotFound));
        var accountId = Guid.NewGuid();
        var foreignStaff = SeedStaff(context, Guid.NewGuid(), "Foreign");
        var writer = new StaffQualificationWriter(context, Principal(accountId), Mock.Of<IAlertRecorder>());

        // Cross-account references surface as 404, never as a permission hint (AC1).
        await Assert.ThrowsAsync<NotFoundException>(() =>
            writer.CreateStaffQualificationAsync(Dto(accountId, foreignStaff.StaffId), CancellationToken.None));
    }

    [Test]
    public async Task Create_WithForeignDocumentLink_IsNotFound()
    {
        using var context = NewContext(nameof(Create_WithForeignDocumentLink_IsNotFound));
        var accountId = Guid.NewGuid();
        var staff = SeedStaff(context, accountId);
        var foreignDocument = new Document(Guid.NewGuid(), "Staff", staff.StaffId.ToString(), "User", Guid.NewGuid().ToString(),
            "LocalFileSystem", "key", "application/pdf", 10, "hash", "Internal", "Active", null, "Account", "Clean", "a.pdf", "License");
        context.Documents.Add(foreignDocument);
        context.SaveChanges();

        var writer = new StaffQualificationWriter(context, Principal(accountId), Mock.Of<IAlertRecorder>());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            writer.CreateStaffQualificationAsync(Dto(accountId, staff.StaffId, foreignDocument.DocumentId), CancellationToken.None));
    }

    [Test]
    public async Task Delete_RemovesRow_AndKeepsBeforeImageInAudit()
    {
        using var context = NewContext(nameof(Delete_RemovesRow_AndKeepsBeforeImageInAudit));
        var accountId = Guid.NewGuid();
        var staff = SeedStaff(context, accountId);
        var writer = new StaffQualificationWriter(context, Principal(accountId), Mock.Of<IAlertRecorder>());
        var created = await writer.CreateStaffQualificationAsync(Dto(accountId, staff.StaffId), CancellationToken.None);

        await writer.DeleteStaffQualificationAsync(created.StaffQualificationId, CancellationToken.None);

        var audit = context.AuditEvents.Single(x => x.Action == "DeleteStaffQualification");
        Assert.Multiple(() =>
        {
            Assert.That(context.StaffQualifications.Count(), Is.EqualTo(0));
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
        var staff = SeedStaff(context, accountId);
        var writer = new StaffQualificationWriter(context, Principal(accountId), Mock.Of<IAlertRecorder>());

        var awkward = "Renewed at RUNT\nPending \"photo\"\tand a backslash \\ plus ñ";
        var dto = Dto(accountId, staff.StaffId) with { Notes = awkward, IssuingAuthority = "Line1\r\nLine2" };
        await writer.CreateStaffQualificationAsync(dto, CancellationToken.None);

        var audit = context.AuditEvents.Single(x => x.Action == "CreateStaffQualification");
        using var parsed = JsonDocument.Parse(audit.NewValuesJson!);
        Assert.That(parsed.RootElement.GetProperty("issuingAuthority").GetString(), Is.EqualTo("Line1\r\nLine2"));
    }

    [Test]
    public async Task Update_AcrossAccounts_IsForbidden()
    {
        using var context = NewContext(nameof(Update_AcrossAccounts_IsForbidden));
        var accountId = Guid.NewGuid();
        var staff = SeedStaff(context, accountId);
        var writer = new StaffQualificationWriter(context, Principal(accountId), Mock.Of<IAlertRecorder>());
        var created = writer.CreateStaffQualificationAsync(Dto(accountId, staff.StaffId), CancellationToken.None).Result;

        var moved = Dto(Guid.NewGuid(), staff.StaffId);
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            writer.UpdateStaffQualificationAsync(created.StaffQualificationId, moved, CancellationToken.None));
    }

    [Test]
    public async Task Update_WithANewExpiry_ResolvesTheOpenExpiryAlerts()
    {
        using var context = NewContext(nameof(Update_WithANewExpiry_ResolvesTheOpenExpiryAlerts));
        var accountId = Guid.NewGuid();
        var staff = SeedStaff(context, accountId);
        var alerts = new Mock<IAlertRecorder>();
        var writer = new StaffQualificationWriter(context, Principal(accountId), alerts.Object);
        var created = await writer.CreateStaffQualificationAsync(Dto(accountId, staff.StaffId), CancellationToken.None);

        await writer.UpdateStaffQualificationAsync(created.StaffQualificationId, Dto(accountId, staff.StaffId) with { ExpiresAt = new DateOnly(2028, 1, 1) }, CancellationToken.None);

        alerts.Verify(a => a.ResolveOpenAsync(accountId, "StaffQualification", created.StaffQualificationId.ToString(),
            It.Is<IReadOnlyCollection<string>>(t => t.Contains(AlertEventTypes.StaffQualificationExpiring) && t.Contains(AlertEventTypes.StaffQualificationExpired)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Update_KeepingTheExpiry_LeavesTheAlertsAlone()
    {
        using var context = NewContext(nameof(Update_KeepingTheExpiry_LeavesTheAlertsAlone));
        var accountId = Guid.NewGuid();
        var staff = SeedStaff(context, accountId);
        var alerts = new Mock<IAlertRecorder>();
        var writer = new StaffQualificationWriter(context, Principal(accountId), alerts.Object);
        var created = await writer.CreateStaffQualificationAsync(Dto(accountId, staff.StaffId), CancellationToken.None);

        await writer.UpdateStaffQualificationAsync(created.StaffQualificationId, Dto(accountId, staff.StaffId) with { Number = "LIC-2" }, CancellationToken.None);

        alerts.Verify(a => a.ResolveOpenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
