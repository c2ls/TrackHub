using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Interfaces.Telemetry;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsProviderHealthSummaryReport(
    IGpsManagerReader manager,
    IGpsTelemetryReader telemetry) : IReport
{
    public string ReportCode => Reports.GpsProviderHealthSummary;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var lookbackHours = filters.GetNumber(FilterNames.LookbackHours) is { } hours && hours > 0
            ? (int)Math.Min(hours, 24 * 90)
            : 24;

        var operators = await manager.GetOperatorsAsync(cancellationToken);
        var rows = new List<GpsProviderHealthRowVm>(operators.Count);
        foreach (var op in operators)
        {
            var summary = await telemetry.GetOperatorHealthSummaryAsync(op.OperatorId, lookbackHours, cancellationToken);
            var status = summary.OfflineChecks > 0 && summary.HealthyChecks == 0
                ? "Offline"
                : summary.FailureCount > 0 ? "Degraded" : "Healthy";
            rows.Add(new GpsProviderHealthRowVm(
                op.OperatorId,
                op.Name,
                status,
                summary.UptimePercent,
                summary.AverageLatencyMs,
                summary.FailureCount,
                summary.LastCheckAt,
                summary.LastFailureCode));
        }
        return ReportDataset.Create(filters, rows);
    }
}
