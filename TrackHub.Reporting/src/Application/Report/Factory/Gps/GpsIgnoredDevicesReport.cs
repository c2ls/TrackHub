using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsIgnoredDevicesReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsManagerReader manager) : IReport
{
    public string ReportCode => Reports.GpsIgnoredDevices;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var accountId = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsIntegration, cancellationToken);
        Guid? operatorId = filters.GetGuid(FilterNames.Operator);
        var devices = await manager.GetSynchronizedDevicesAsync(accountId, "IGNORED", operatorId, cancellationToken);
        var operators = (await manager.GetOperatorsAsync(cancellationToken))
            .ToDictionary(o => o.OperatorId, o => o.Name);
        var rows = devices
            .Select(d => new GpsIgnoredDeviceRowVm(
                d.DeviceId,
                d.Identifier,
                d.Serial,
                operators.TryGetValue(d.OperatorId, out var n) ? n : d.OperatorId.ToString(),
                d.IgnoredAt))
            .ToList();
        return ReportDataset.Create(filters, rows);
    }
}
