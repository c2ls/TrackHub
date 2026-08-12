namespace TrackHub.Reporting.Domain.Exceptions;

public sealed class ReportLimitExceededException : Exception
{
    public ReportLimitExceededException(int maxRows)
        : base($"Report exceeds the maximum allowed row count of {maxRows}. Please narrow your filters.")
        => MaxRows = maxRows;

    private ReportLimitExceededException(int maxRows, string message)
        : base(message)
        => MaxRows = maxRows;

    public string Code => "REPORT_ROW_LIMIT_EXCEEDED";
    public int MaxRows { get; }

    // PDF is capped separately: oversized datasets return 400 advising Excel, never a
    // truncated PDF.
    public static ReportLimitExceededException ForPdf(int maxRows)
        => new(maxRows, $"PDF export is limited to {maxRows} rows. Please export this report as Excel (xlsx) instead.");
}
