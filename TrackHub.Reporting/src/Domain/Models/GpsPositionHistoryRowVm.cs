namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsPositionHistoryRowVm(
    string TransporterName,
    DateTimeOffset DeviceDateTime,
    double Latitude,
    double Longitude,
    string Serial);
