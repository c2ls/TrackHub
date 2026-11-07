namespace TrackHub.Manager.Domain.Models;

public readonly record struct AccountHolidayVm(Guid AccountHolidayId, Guid AccountId, DateOnly Date, string Name);
