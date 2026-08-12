namespace TrackHub.Manager.Domain.Interfaces;

public interface IDriverQualificationReader
{
    /// <summary>
    /// Account-scoped qualification list. <paramref name="expiringWithinDays"/> narrows to rows whose
    /// <c>ExpiresAt</c> falls on or before that many days from today (past-due rows always qualify),
    /// which is what drives the portal's expirations view.
    /// </summary>
    Task<IReadOnlyCollection<DriverQualificationVm>> GetDriverQualificationsAsync(Guid accountId, Guid? driverId, int? expiringWithinDays, int skip, int take, CancellationToken cancellationToken);
}
