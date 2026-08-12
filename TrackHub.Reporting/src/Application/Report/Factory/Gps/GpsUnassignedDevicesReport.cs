using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsUnassignedDevicesReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsManagerReader manager) : IReport
{
    public string ReportCode => Reports.GpsUnassignedDevices;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var accountId = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsIntegration, cancellationToken);
        var devices = await manager.GetUnassignedDevicesAsync(accountId, cancellationToken);
        var operators = (await manager.GetOperatorsAsync(cancellationToken))
            .ToDictionary(o => o.OperatorId, o => o.Name);
        var rows = devices
            .Select(d => new GpsUnassignedDeviceRowVm(
                d.DeviceId,
                d.Identifier,
                d.Serial,
                operators.TryGetValue(d.OperatorId, out var n) ? n : d.OperatorId.ToString(),
                d.ProviderStatus,
                d.FirstSeenAt,
                d.LastSeenAt))
            .ToList();
        return ReportDataset.Create(filters, rows);
    }
}
