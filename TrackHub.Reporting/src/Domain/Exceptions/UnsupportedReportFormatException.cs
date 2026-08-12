namespace TrackHub.Reporting.Domain.Exceptions;

// An unknown format was requested, or PDF was requested for an Excel-only report.
public sealed class UnsupportedReportFormatException : Exception
{
    public UnsupportedReportFormatException(string format)
        : base($"Report format '{format}' is not supported. Use 'xlsx' or 'pdf'.")
        => Format = format;

    private UnsupportedReportFormatException(string format, string message)
        : base(message)
        => Format = format;

    public string Code => "UNSUPPORTED_REPORT_FORMAT";
    public string Format { get; }

    // Factory for the "this report is Excel-only" case (SupportsPdf = false).
    public static UnsupportedReportFormatException ExcelOnly(string format)
        => new(format, "This report supports Excel export only. Please export it as 'xlsx'.");
}
