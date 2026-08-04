using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsRecentlyAddedDevicesReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsManagerReader manager) : IReport
{
    public string ReportCode => Reports.GpsRecentlyAddedDevices;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var accountId = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsIntegration, cancellationToken);
        var lookbackDays = filters.GetNumber(FilterNames.WithinDays) is { } days && days > 0
            ? (int)Math.Min(days, 365)
            : 7;
        var cutoff = filters.GetDate(FilterNames.From) ?? DateTimeOffset.UtcNow.AddDays(-lookbackDays);
        var devices = await manager.GetSynchronizedDevicesAsync(accountId, null, null, cancellationToken);
        var operators = (await manager.GetOperatorsAsync(cancellationToken))
            .ToDictionary(o => o.OperatorId, o => o.Name);
        var rows = devices
            .Where(d => d.FirstSeenAt >= cutoff)
            .OrderByDescending(d => d.FirstSeenAt)
            .Select(d => new GpsRecentlyAddedDeviceRowVm(
                d.DeviceId,
                d.Identifier,
                d.Serial,
                operators.TryGetValue(d.OperatorId, out var n) ? n : d.OperatorId.ToString(),
                d.ProviderDisplayName,
                d.FirstSeenAt))
            .ToList();
        return ReportDataset.Create(filters, rows);
    }
}
