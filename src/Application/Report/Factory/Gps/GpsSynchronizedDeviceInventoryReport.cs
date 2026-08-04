using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsSynchronizedDeviceInventoryReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsManagerReader manager) : IReport
{
    public string ReportCode => Reports.GpsSynchronizedDeviceInventory;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var accountId = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsIntegration, cancellationToken);
        Guid? operatorId = filters.GetGuid(FilterNames.Operator);
        var devices = await manager.GetSynchronizedDevicesAsync(accountId, null, operatorId, cancellationToken);
        var operators = (await manager.GetOperatorsAsync(cancellationToken))
            .ToDictionary(o => o.OperatorId, o => o.Name);
        var assignments = (await manager.GetAssignmentsByAccountAsync(accountId, true, cancellationToken))
            .GroupBy(a => a.DeviceId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.IsPrimary).ThenByDescending(a => a.EffectiveFrom).First());

        var rows = devices.Select(d =>
        {
            assignments.TryGetValue(d.DeviceId, out var assignment);
            return new GpsSynchronizedDeviceInventoryRowVm(
                d.DeviceId,
                d.Identifier,
                d.Serial,
                operators.TryGetValue(d.OperatorId, out var n) ? n : d.OperatorId.ToString(),
                d.ProviderStatus,
                d.DetectedStatus,
                d.FirstSeenAt,
                d.LastSeenAt,
                assignment.TransporterDeviceAssignmentId == Guid.Empty ? null : assignment.TransporterId.ToString(),
                assignment.TransporterDeviceAssignmentId == Guid.Empty ? null : assignment.EffectiveFrom);
        }).ToList();
        return ReportDataset.Create(filters, rows);
    }
}
