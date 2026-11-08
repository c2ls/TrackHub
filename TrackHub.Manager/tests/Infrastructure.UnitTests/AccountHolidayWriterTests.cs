using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class AccountHolidayWriterTests
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

    [Test]
    public async Task Set_ReplacesTheYear_DedupesDates_AndAudits()
    {
        using var context = NewContext(nameof(Set_ReplacesTheYear_DedupesDates_AndAudits));
        var accountId = Guid.NewGuid();
        var principal = Principal(accountId);
        var writer = new AccountHolidayWriter(context, principal);
        var reader = new AccountHolidayReader(context, principal);

        await writer.SetAccountHolidaysAsync(accountId, 2026, [new AccountHolidayDto(new DateOnly(2026, 1, 1), "New Year"), new AccountHolidayDto(new DateOnly(2026, 12, 25), "Christmas")], CancellationToken.None);
        await writer.SetAccountHolidaysAsync(accountId, 2027, [new AccountHolidayDto(new DateOnly(2027, 1, 1), "New Year")], CancellationToken.None);
        var replaced = await writer.SetAccountHolidaysAsync(accountId, 2026,
            [new AccountHolidayDto(new DateOnly(2026, 5, 1), "Labour"), new AccountHolidayDto(new DateOnly(2026, 5, 1), "Labour Day"), new AccountHolidayDto(new DateOnly(2025, 5, 1), "Wrong year")],
            CancellationToken.None);

        var stored = await reader.GetAccountHolidaysAsync(accountId, 2026, CancellationToken.None);
        var untouched = await reader.GetAccountHolidaysAsync(accountId, 2027, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(replaced.Select(h => h.Name), Is.EqualTo(new[] { "Labour Day" }));
            Assert.That(stored.Select(h => h.Date), Is.EqualTo(new[] { new DateOnly(2026, 5, 1) }));
            Assert.That(untouched, Has.Count.EqualTo(1));
            Assert.That(context.AuditEvents.Count(x => x.Action == "SetAccountHolidays"), Is.EqualTo(3));
        });
    }

    [Test]
    public async Task Set_ForAnotherAccount_IsForbidden()
    {
        using var context = NewContext(nameof(Set_ForAnotherAccount_IsForbidden));
        var writer = new AccountHolidayWriter(context, Principal(Guid.NewGuid()));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.SetAccountHolidaysAsync(Guid.NewGuid(), 2026, [], CancellationToken.None));
    }
}
