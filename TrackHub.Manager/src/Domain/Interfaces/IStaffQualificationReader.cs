namespace TrackHub.Manager.Domain.Interfaces;

public interface IStaffQualificationReader
{
    /// <summary>
    /// Account-scoped qualification list. <paramref name="expiringWithinDays"/> narrows to rows whose
    /// <c>ExpiresAt</c> falls on or before that many days from today (past-due rows always qualify),
    /// which is what drives the portal's expirations view.
    /// </summary>
    Task<StaffQualificationsPageVm> GetStaffQualificationsAsync(Guid accountId, Guid? staffId, int? expiringWithinDays, int skip, int take, CancellationToken cancellationToken);
}
