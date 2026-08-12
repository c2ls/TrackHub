using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Domain.Interfaces.Router;

public interface IRouterReader
{
    Task<IEnumerable<PositionVm>> GetDevicePositionsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<PositionVm>> GetPositionsRecordAsync(FilterDto filters, CancellationToken cancellationToken);
}

