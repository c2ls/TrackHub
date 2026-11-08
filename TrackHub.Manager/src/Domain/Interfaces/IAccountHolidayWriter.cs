namespace TrackHub.Manager.Domain.Interfaces;

public interface IAccountHolidayWriter
{
    Task<IReadOnlyCollection<AccountHolidayVm>> SetAccountHolidaysAsync(Guid accountId, int year, IReadOnlyCollection<AccountHolidayDto> holidays, CancellationToken cancellationToken);
}
