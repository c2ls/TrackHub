using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Domain.Enums;

namespace TrackHub.Manager.Domain.Interfaces;

public interface ITransporterDeviceAssignmentReader
{
    Task<TransporterDeviceAssignmentVm> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<TransporterDeviceAssignmentsPageVm> GetByTransporterAsync(Guid transporterId, bool activeOnly, int skip, int take, CancellationToken cancellationToken);
    Task<TransporterDeviceAssignmentsPageVm> GetByAccountAsync(Guid accountId, bool activeOnly, int skip, int take, CancellationToken cancellationToken);
}
