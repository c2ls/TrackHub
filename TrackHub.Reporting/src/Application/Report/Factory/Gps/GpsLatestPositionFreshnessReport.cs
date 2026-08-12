using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Interfaces.Telemetry;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsLatestPositionFreshnessReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsManagerReader manager,
    IGpsTelemetryReader telemetry) : IReport
{
    public string ReportCode => Reports.GpsLatestPositionFreshness;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        _ = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsIntegration, cancellationToken);
        var operators = await manager.GetOperatorsAsync(cancellationToken);
        var rows = new List<GpsLatestPositionFreshnessRowVm>();
        var now = DateTimeOffset.UtcNow;
        foreach (var op in operators.Where(o => o.Enabled))
        {
            IReadOnlyCollection<Domain.Models.Manager.ManagerTransporterPositionVm> positions;
            try
            {
                positions = await telemetry.GetLatestPositionsAsync(op.OperatorId, cancellationToken);
            }
            catch
            {
                continue;
            }
            foreach (var p in positions)
            {
                rows.Add(new GpsLatestPositionFreshnessRowVm(
                    p.TransporterId,
                    p.DeviceName,
                    p.DeviceDateTime,
                    Math.Round((now - p.DeviceDateTime).TotalMinutes, 2),
                    op.Name));
            }
        }
        return ReportDataset.Create(filters, rows);
    }
}
