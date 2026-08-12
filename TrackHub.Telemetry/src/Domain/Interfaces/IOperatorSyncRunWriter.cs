using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Domain.Records;

namespace TrackHub.Telemetry.Domain.Interfaces;

public interface IOperatorSyncRunWriter
{
    Task<OperatorSyncRunVm> RecordAsync(OperatorSyncRunDto dto, CancellationToken cancellationToken);
}
