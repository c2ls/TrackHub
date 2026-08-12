namespace TrackHub.Reporting.Domain.Models;

public record struct TransporterInGeofenceVm(
    Guid TransporterId,
    string TransporterName,
    Guid GeofenceId,
    string GeofenceName);

