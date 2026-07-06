using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Domain.Records;
using Common.Domain.Helpers;

namespace TrackHub.Telemetry.Domain.Interfaces;

public interface ITransporterPositionHistoryReader
{
    Task<IReadOnlyCollection<TransporterPositionHistoryVm>> GetAsync(Filters filters, int take, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TransporterPositionHistoryVm>> GetRangeAsync(Guid accountId, Guid transporterId, DateTimeOffset from, DateTimeOffset to, int maxPoints, CancellationToken cancellationToken);
}
