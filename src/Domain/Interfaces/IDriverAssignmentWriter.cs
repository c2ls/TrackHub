namespace TrackHub.Manager.Domain.Interfaces;

public interface IDriverAssignmentWriter
{
    Task<DriverTransporterAssignmentVm> AssignDriverToTransporterAsync(Guid driverId, Guid transporterId, DateTimeOffset startsAt, string assignmentType, CancellationToken cancellationToken);

    /// <summary>Closes an open assignment. Ended assignments are immutable — re-ending one is rejected.</summary>
    Task EndDriverAssignmentAsync(Guid driverTransporterAssignmentId, DateTimeOffset? endsAt, CancellationToken cancellationToken);
}
