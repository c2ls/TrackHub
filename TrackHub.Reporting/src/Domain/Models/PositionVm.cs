namespace TrackHub.Reporting.Domain.Models;

public readonly record struct PositionVm(
    Guid TransporterId,
    string DeviceName,
    [property: ReportValues("TransporterType")] string TransporterType,
    double Latitude,
    double Longitude,
    double? Altitude,
    DateTimeOffset DeviceDateTime,
    DateTimeOffset? ServerDateTime,
    double Speed,
    double? Course,
    int? EventId,
    string? Address,
    string? City,
    string? State,
    string? Country
    );
