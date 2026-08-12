using TrackHub.Manager.Domain.Records;

namespace TrackHub.Manager.Domain.Interfaces;

public interface IReportWriter
{
    Task UpdateReportAsync(UpdateReportDto reportDto, CancellationToken cancellationToken);
}
