using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Interfaces.Telemetry;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Options;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsSyncStatisticsReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsManagerReader manager,
    IGpsTelemetryReader telemetry,
    ReportingLimitsOptions limits) : IReport
{
    public string ReportCode => Reports.GpsSyncStatistics;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var accountId = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsIntegration, cancellationToken);
        var take = GpsReportSupport.ResolveTake(filters, limits);
        var runs = await telemetry.GetOperatorSyncRunsAsync(accountId, null, take, cancellationToken);
        var operators = (await manager.GetOperatorsAsync(cancellationToken))
            .ToDictionary(o => o.OperatorId, o => o.Name);

        IEnumerable<Domain.Models.Manager.ManagerOperatorSyncRunVm> filtered = runs;
        if (filters.GetDate(FilterNames.From) is { } from)
            filtered = filtered.Where(r => r.StartedAt >= from);
        if (filters.GetDate(FilterNames.To) is { } to)
            filtered = filtered.Where(r => r.StartedAt <= to);

        var rows = filtered
            .GroupBy(r => new { Date = new DateTimeOffset(r.StartedAt.UtcDateTime.Date, TimeSpan.Zero), r.OperatorId })
            .Select(g =>
            {
                var successes = g.Count(x => string.Equals(x.Result, "SUCCESS", StringComparison.OrdinalIgnoreCase));
                var durations = g
                    .Where(x => x.CompletedAt.HasValue)
                    .Select(x => (x.CompletedAt!.Value - x.StartedAt).TotalMilliseconds)
                    .ToArray();
                return new GpsSyncStatisticsRowVm(
                    g.Key.Date,
                    operators.TryGetValue(g.Key.OperatorId, out var n) ? n : g.Key.OperatorId.ToString(),
                    g.Count(),
                    successes,
                    g.Count() - successes,
                    durations.Length > 0 ? durations.Average() : 0,
                    g.Sum(x => x.PositionsAccepted));
            })
            .OrderBy(r => r.Date)
            .ThenBy(r => r.Operator)
            .ToList();
        return ReportDataset.Create(filters, rows);
    }
}
