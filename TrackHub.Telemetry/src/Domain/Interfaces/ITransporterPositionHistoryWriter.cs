using TrackHub.Telemetry.Domain.Records;

namespace TrackHub.Telemetry.Domain.Interfaces;

public interface ITransporterPositionHistoryWriter
{
    Task<bool> AppendAsync(TransporterPositionHistoryDto dto, CancellationToken cancellationToken);
    Task<int> AppendRangeAsync(IReadOnlyCollection<TransporterPositionHistoryDto> dtos, CancellationToken cancellationToken);
}
