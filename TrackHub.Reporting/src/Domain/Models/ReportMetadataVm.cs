namespace TrackHub.Reporting.Domain.Models;

// Governed-catalog metadata for a report, read from the Manager catalog (reportByCode) and used by the
// execution pipeline to enforce feature/role visibility at run time.
public readonly record struct ReportMetadataVm(
    string Code,
    string? Description,
    string Category,
    string? RequiredFeatureKey,
    bool ManagerOnly,
    bool SupportsPdf,
    int SortOrder,
    bool Active);
