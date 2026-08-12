using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Domain.Interfaces;

// Reads governed-catalog metadata for a single report code from the Manager catalog.
// Returns null when the code is unknown. Implementations cache results (hits and misses).
public interface IReportCatalogReader
{
    Task<ReportMetadataVm?> GetReportByCodeAsync(string code, CancellationToken cancellationToken);
}
