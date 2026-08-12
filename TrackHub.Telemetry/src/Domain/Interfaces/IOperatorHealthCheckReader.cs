using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Domain.Records;

namespace TrackHub.Telemetry.Domain.Interfaces;

public interface IOperatorHealthCheckReader
{
    Task<IReadOnlyCollection<OperatorHealthCheckVm>> GetByOperatorAsync(Guid operatorId, int take, CancellationToken cancellationToken);
    Task<OperatorHealthVm> GetLatestHealthAsync(Guid operatorId, CancellationToken cancellationToken);
    Task<OperatorHealthSummaryVm> GetSummaryAsync(Guid operatorId, DateTimeOffset since, CancellationToken cancellationToken);
}
