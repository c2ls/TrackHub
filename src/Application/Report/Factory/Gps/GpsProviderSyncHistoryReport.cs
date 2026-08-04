using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Telemetry;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Options;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsProviderSyncHistoryReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsTelemetryReader telemetry,
    ReportingLimitsOptions limits) : IReport
{
    public string ReportCode => Reports.GpsProviderSyncHistory;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var accountId = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsIntegration, cancellationToken);
        Guid? operatorId = filters.GetGuid(FilterNames.Operator);
        var take = GpsReportSupport.ResolveTake(filters, limits, 500);
        var runs = await telemetry.GetOperatorSyncRunsAsync(accountId, operatorId, take, cancellationToken);
        IEnumerable<Domain.Models.Manager.ManagerOperatorSyncRunVm> filtered = runs;
        if (filters.GetDate(FilterNames.From) is { } from)
            filtered = filtered.Where(r => r.StartedAt >= from);
        if (filters.GetDate(FilterNames.To) is { } to)
            filtered = filtered.Where(r => r.StartedAt <= to);
        var rows = filtered.Select(r => new GpsProviderSyncHistoryRowVm(
            r.OperatorId,
            r.StartedAt,
            r.CompletedAt,
            r.TriggerType,
            r.Result,
            r.DevicesSeen,
            r.DevicesAdded,
            r.DevicesUpdated,
            r.DevicesRemoved,
            r.DevicesIgnored,
            Math.Max(0, r.DevicesSeen - r.DevicesAdded - r.DevicesUpdated - r.DevicesIgnored),
            r.PositionsRead,
            r.CorrelationId)).ToList();
        return ReportDataset.Create(filters, rows);
    }
}
