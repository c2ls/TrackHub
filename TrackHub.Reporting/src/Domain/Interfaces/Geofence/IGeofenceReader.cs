using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Domain.Interfaces.Geofence;

public interface IGeofenceReader
{
    Task<IEnumerable<TransporterInGeofenceVm>> GetTransportersInGeofenceAsync(CancellationToken cancellationToken);
    Task<IEnumerable<GeofenceEventReportVm>> GetGeofenceEventsAsync(FilterDto filters, CancellationToken cancellationToken);
}

