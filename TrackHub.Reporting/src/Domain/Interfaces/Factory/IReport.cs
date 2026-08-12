using TrackHub.Reporting.Domain.Records;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Domain.Interfaces.Factory;

public interface IReport
{
    string ReportCode { get; }

    // Fetches the report's data and projects it into a format-agnostic dataset. The execution pipeline
    // then renders it (Excel/PDF) or serializes it (preview).
    Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken);
}
