namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class TransporterPositionHistory(
    Guid accountId,
    Guid operatorId,
    Guid deviceId,
    Guid transporterId,
    DateTimeOffset sourceTimestamp,
    DateTimeOffset receivedAt,
    double latitude,
    double longitude,
    double? altitude,
    double speed,
    double? course,
    int? eventId,
    string? address,
    string? city,
    string? state,
    string? country,
    string? attributes,
    string idempotencyKey)
{
    public Guid TransporterPositionHistoryId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public Guid OperatorId { get; set; } = operatorId;
    public Guid DeviceId { get; set; } = deviceId;
    public Guid TransporterId { get; set; } = transporterId;
    public DateTimeOffset SourceTimestamp { get; set; } = sourceTimestamp;
    public DateTimeOffset ReceivedAt { get; set; } = receivedAt;
    public double Latitude { get; set; } = latitude;
    public double Longitude { get; set; } = longitude;
    public double? Altitude { get; set; } = altitude;
    public double Speed { get; set; } = speed;
    public double? Course { get; set; } = course;
    public int? EventId { get; set; } = eventId;
    public string? Address { get; set; } = address;
    public string? City { get; set; } = city;
    public string? State { get; set; } = state;
    public string? Country { get; set; } = country;
    public string? Attributes { get; set; } = attributes;
    public string IdempotencyKey { get; set; } = idempotencyKey;
}
