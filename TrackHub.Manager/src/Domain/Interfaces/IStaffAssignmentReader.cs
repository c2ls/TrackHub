namespace TrackHub.Manager.Domain.Interfaces;

public interface IStaffAssignmentReader
{
    Task<StaffAssignmentHistoryPageVm> GetStaffAssignmentHistoryAsync(Guid accountId, Guid? staffId, Guid? transporterId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken);
}
