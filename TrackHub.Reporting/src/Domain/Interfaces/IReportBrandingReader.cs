using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Domain.Interfaces;

// Reads the caller-account's branding (display name + logo) from the Manager catalog to brand PDF exports
//. Implementations are null-tolerant and cache results: a branding
// lookup that fails or is absent returns null so the export proceeds unbranded rather than erroring.
public interface IReportBrandingReader
{
    Task<ReportBrandingVm?> GetBrandingAsync(Guid accountId, CancellationToken cancellationToken);
}
