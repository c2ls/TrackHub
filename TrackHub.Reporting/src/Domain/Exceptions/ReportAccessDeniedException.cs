namespace TrackHub.Reporting.Domain.Exceptions;

// A manager-only report was invoked by a caller lacking the Administrator/Manager role (403).
public sealed class ReportAccessDeniedException(string reportCode)
    : Exception($"Report '{reportCode}' requires a manager role.")
{
    public string Code => "REPORT_ACCESS_DENIED";
    public string ReportCode { get; } = reportCode;
}
