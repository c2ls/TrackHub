using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Domain.Records;

namespace TrackHub.Manager.Domain.Interfaces;

public interface ITransporterDeviceAssignmentWriter
{
    Task<TransporterDeviceAssignmentVm> AssignAsync(TransporterDeviceAssignmentDto dto, CancellationToken cancellationToken);
    Task EndAssignmentAsync(Guid assignmentId, string? reason, CancellationToken cancellationToken);
}
