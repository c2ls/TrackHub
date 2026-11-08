namespace TrackHub.Manager.Domain.Interfaces;

public interface IStaffAssignmentWriter
{
    Task<StaffTransporterAssignmentVm> AssignStaffToTransporterAsync(Guid staffId, Guid transporterId, DateTimeOffset startsAt, string assignmentType, CancellationToken cancellationToken);

    /// <summary>Closes an open assignment. Ended assignments are immutable — re-ending one is rejected.</summary>
    Task EndStaffAssignmentAsync(Guid staffTransporterAssignmentId, DateTimeOffset? endsAt, CancellationToken cancellationToken);
}
