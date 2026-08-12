namespace TrackHub.Reporting.Domain.Models;

// One preview column: the resx key (Name) and the culture-resolved header text (Header).
public readonly record struct ReportPreviewColumn(string Name, string Header);

// On-screen preview payload: the first PreviewRows rows plus accurate totals/truncation metadata so
// the portal can show a paged table before the user commits to a download.
public readonly record struct ReportPreviewVm(
    IReadOnlyList<ReportPreviewColumn> Columns,
    IReadOnlyList<object?[]> Rows,
    int TotalRows,
    bool Truncated);
