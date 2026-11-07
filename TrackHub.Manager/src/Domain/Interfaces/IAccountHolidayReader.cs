namespace TrackHub.Manager.Domain.Interfaces;

public interface IAccountHolidayReader
{
    Task<IReadOnlyCollection<AccountHolidayVm>> GetAccountHolidaysAsync(Guid accountId, int year, CancellationToken cancellationToken);
}
