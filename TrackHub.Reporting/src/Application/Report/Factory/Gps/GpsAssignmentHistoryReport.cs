using TrackHub.Reporting.Domain.Helpers;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Gps;

public sealed class GpsAssignmentHistoryReport(
    IUser user,
    IAccountFeatureReader features,
    IGpsManagerReader manager,
    IReportNameReader names) : IReport
{
    public string ReportCode => Reports.GpsAssignmentHistory;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var accountId = await GpsReportSupport.RequireAccountAsync(user, features, FeatureKeys.GpsIntegration, cancellationToken);
        var assignments = await manager.GetAssignmentsByAccountAsync(accountId, false, cancellationToken);
        IEnumerable<Domain.Models.Manager.ManagerTransporterDeviceAssignmentVm> filtered = assignments;
        if (filters.GetGuid(FilterNames.Transporter) is { } transporterId)
            filtered = filtered.Where(a => a.TransporterId == transporterId);
        if (filters.GetDate(FilterNames.From) is { } from)
            filtered = filtered.Where(a => a.EffectiveFrom >= from);
        if (filters.GetDate(FilterNames.To) is { } to)
            filtered = filtered.Where(a => a.EffectiveFrom <= to);
        var transporters = await names.GetTransporterNamesAsync(filtered.Select(a => a.TransporterId), cancellationToken);
        var devices = await names.GetDeviceSerialsAsync(accountId, cancellationToken);
        var rows = filtered
            .OrderByDescending(a => a.EffectiveFrom)
            .Select(a => new GpsAssignmentHistoryRowVm(
                transporters.NameOf(a.TransporterId),
                devices.NameOf(a.DeviceId),
                a.IsPrimary ? "Primary" : "Secondary",
                a.EffectiveFrom,
                a.EffectiveTo,
                a.AssignmentReason))
            .ToList();
        return ReportDataset.Create(filters, rows);
    }
}
