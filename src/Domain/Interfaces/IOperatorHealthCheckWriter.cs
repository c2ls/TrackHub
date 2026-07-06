using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Domain.Records;

namespace TrackHub.Telemetry.Domain.Interfaces;

public interface IOperatorHealthCheckWriter
{
    Task<OperatorHealthCheckVm> RecordAsync(OperatorHealthCheckDto dto, CancellationToken cancellationToken);
}
