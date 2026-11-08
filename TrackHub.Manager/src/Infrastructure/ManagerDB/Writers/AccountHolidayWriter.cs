using System.Globalization;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class AccountHolidayWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IAccountHolidayWriter
{
    public async Task<IReadOnlyCollection<AccountHolidayVm>> SetAccountHolidaysAsync(Guid accountId, int year, IReadOnlyCollection<AccountHolidayDto> holidays, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountWriteAccess(accountId);
        var existing = await Context.AccountHolidays.AsTracking()
            .Where(h => h.AccountId == scopedAccountId && h.Date.Year == year)
            .OrderBy(h => h.Date)
            .ToListAsync(cancellationToken);
        var oldValues = Describe(existing);
        Context.AccountHolidays.RemoveRange(existing);

        var replacement = holidays
            .Where(h => h.Date.Year == year)
            .GroupBy(h => h.Date)
            .OrderBy(g => g.Key)
            .Select(g => new AccountHoliday(scopedAccountId, g.Key, g.Last().Name.Trim()))
            .ToList();
        await Context.AccountHolidays.AddRangeAsync(replacement, cancellationToken);

        Context.AuditEvents.Add(AuditTrail.Create(Principal, scopedAccountId, "SetAccountHolidays", "AccountHoliday",
            year.ToString(CultureInfo.InvariantCulture), oldValues, Describe(replacement)));
        await Context.SaveChangesAsync(cancellationToken);
        return replacement.Select(h => new AccountHolidayVm(h.AccountHolidayId, h.AccountId, h.Date, h.Name)).ToList();
    }

    private static string Describe(IEnumerable<AccountHoliday> holidays)
        => "[" + string.Join(",", holidays.Select(h => $$"""{"date":"{{h.Date:O}}","name":{{AuditJson.Quote(h.Name)}}}""")) + "]";
}
