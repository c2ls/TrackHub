using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class AccountHolidayReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IAccountHolidayReader
{
    public async Task<IReadOnlyCollection<AccountHolidayVm>> GetAccountHolidaysAsync(Guid accountId, int year, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);
        return await Context.AccountHolidays
            .Where(h => h.AccountId == scopedAccountId && h.Date.Year == year)
            .OrderBy(h => h.Date).ThenBy(h => h.AccountHolidayId)
            .Select(h => new AccountHolidayVm(h.AccountHolidayId, h.AccountId, h.Date, h.Name))
            .ToListAsync(cancellationToken);
    }
}
