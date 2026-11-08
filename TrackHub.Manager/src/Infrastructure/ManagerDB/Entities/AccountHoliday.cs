using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class AccountHoliday(Guid accountId, DateOnly date, string name) : BaseAuditableEntity
{
    public Guid AccountHolidayId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public DateOnly Date { get; set; } = date;
    public string Name { get; set; } = name;
}
