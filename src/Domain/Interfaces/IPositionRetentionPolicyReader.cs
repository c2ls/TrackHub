using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Domain.Records;

namespace TrackHub.Telemetry.Domain.Interfaces;

public interface IPositionRetentionPolicyReader
{
    Task<PositionRetentionPolicyVm> GetAsync(Guid accountId, CancellationToken cancellationToken);
}
