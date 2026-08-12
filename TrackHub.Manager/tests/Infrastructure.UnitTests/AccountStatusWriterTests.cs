using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Enums;
using Moq;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class AccountStatusWriterTests
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

    private static async Task<Guid> SeedAccountAsync(ApplicationDbContext context, AccountStatus status)
    {
        var account = new Account("Acme", "Description", 2, status.IsOperational()) { Status = (short)status };
        await context.Accounts.AddAsync(account);
        await context.SaveChangesAsync(CancellationToken.None);
        return account.AccountId;
    }

    [Test]
    public async Task ChangeStatusAsync_ActiveToSuspended_UpdatesStatusActiveAndTimestamp()
    {
        await using var context = NewContext(nameof(ChangeStatusAsync_ActiveToSuspended_UpdatesStatusActiveAndTimestamp));
        var accountId = await SeedAccountAsync(context, AccountStatus.Active);
        var writer = new AccountStatusWriter(context, Principal(accountId));

        var (vm, previous) = await writer.ChangeStatusAsync(accountId, AccountStatus.Suspended, "Non-payment.", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(previous, Is.EqualTo(AccountStatus.Active));
            Assert.That(vm.Status, Is.EqualTo(AccountStatus.Suspended));
            Assert.That(vm.StatusId, Is.EqualTo((short)AccountStatus.Suspended));
            Assert.That(vm.Active, Is.False); // derived Active follows the non-operational status
        });

        var stored = context.Accounts.Single(a => a.AccountId == accountId);
        Assert.That(stored.Status, Is.EqualTo((short)AccountStatus.Suspended));
        Assert.That(stored.Active, Is.False);
        Assert.That(stored.StatusChangedAt, Is.Not.Null);
    }

    [Test]
    public async Task ChangeStatusAsync_WritesAuditEvent()
    {
        await using var context = NewContext(nameof(ChangeStatusAsync_WritesAuditEvent));
        var accountId = await SeedAccountAsync(context, AccountStatus.Active);
        var writer = new AccountStatusWriter(context, Principal(accountId));

        await writer.ChangeStatusAsync(accountId, AccountStatus.Suspended, "Non-payment.", CancellationToken.None);

        var audit = context.AuditEvents.SingleOrDefault(e => e.Action == "AccountStatusChanged" && e.AccountId == accountId);
        Assert.That(audit, Is.Not.Null);
        Assert.That(audit!.NewValuesJson, Does.Contain("Suspended"));
        Assert.That(audit.OldValuesJson, Does.Contain("Active"));
    }

    [Test]
    public void ChangeStatusAsync_MissingAccount_ThrowsNotFound()
    {
        var context = NewContext(nameof(ChangeStatusAsync_MissingAccount_ThrowsNotFound));
        var writer = new AccountStatusWriter(context, Principal(Guid.NewGuid()));

        Assert.ThrowsAsync<NotFoundException>(async () =>
            await writer.ChangeStatusAsync(Guid.NewGuid(), AccountStatus.Suspended, "x", CancellationToken.None));
    }

    [Test]
    public async Task AccountOperationalStatusReader_ReturnsStoredStatus_AndNullForUnknown()
    {
        await using var context = NewContext(nameof(AccountOperationalStatusReader_ReturnsStoredStatus_AndNullForUnknown));
        var accountId = await SeedAccountAsync(context, AccountStatus.Suspended);
        var reader = new AccountOperationalStatusReader(context as IApplicationDbContext);

        var found = await reader.GetAccountStatusAsync(accountId, CancellationToken.None);
        var missing = await reader.GetAccountStatusAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(found, Is.EqualTo(AccountStatus.Suspended));
        Assert.That(missing, Is.Null);
    }
}
