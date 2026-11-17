using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Helpers;
using TrackHub.Reporting.Domain.Paging;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Telemetry;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Options;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsPositionHistoryReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsTelemetryReader telemetry,
    IReportNameReader names,
    ReportingLimitsOptions limits) : IReport
{
    public string ReportCode => Reports.GpsPositionHistory;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var accountId = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsPositionHistory, cancellationToken);
        Guid? transporterId = filters.GetGuid(FilterNames.Transporter);
        Guid? deviceId = filters.GetGuid(FilterNames.Device);
        var take = GpsReportSupport.ResolveTake(filters, limits);
        // The window goes to the SOURCE and the feed is followed to its end: one capped page was
        // the newest N fixes, so a monthly export of an active fleet covered its last few hours.
        var from = filters.GetDate(FilterNames.From);
        var to = filters.GetDate(FilterNames.To);
        var history = await FeedDrain.DrainByCursorAsync<Domain.Models.Manager.ManagerTransporterPositionHistoryVm>(async cursor =>
        {
            var page = await telemetry.GetPositionHistoryAsync(accountId, transporterId, deviceId, take, from, to, cursor, cancellationToken);
            return (page.Items, page.HasMore, page.NextCursor);
        });

        var transporters = await names.GetTransporterNamesAsync(history.Select(p => p.TransporterId), cancellationToken);
        var devices = await names.GetDeviceSerialsAsync(accountId, cancellationToken);
        var rows = history
            .OrderByDescending(p => p.SourceTimestamp)
            .Select(p => new GpsPositionHistoryRowVm(
                transporters.NameOf(p.TransporterId),
                p.SourceTimestamp,
                p.Latitude,
                p.Longitude,
                devices.NameOf(p.DeviceId)))
            .ToList();
        return ReportDataset.Create(filters, rows);
    }
}
