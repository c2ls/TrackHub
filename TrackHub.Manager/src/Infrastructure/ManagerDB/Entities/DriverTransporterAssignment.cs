using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

/// <summary>
/// Time-bounded driver↔transporter assignment history (spec 09 §6). Authoritative operational record;
/// <c>Driver.DefaultTransporterId</c> stays as the pre-selected default (spec 09 §18.2). At most one
/// open assignment may exist per (driver, transporter) pair — enforced by the writer, which returns
/// 409 on overlap. A driver may hold several concurrent assignments to DIFFERENT transporters.
/// </summary>
public sealed class DriverTransporterAssignment(
    Guid accountId,
    Guid driverId,
    Guid transporterId,
    DateTimeOffset startsAt,
    DateTimeOffset? endsAt,
    string assignmentType,
    string status,
    string createdByPrincipal) : BaseAuditableEntity
{
    public Guid DriverTransporterAssignmentId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public Guid DriverId { get; set; } = driverId;
    public Guid TransporterId { get; set; } = transporterId;
    public DateTimeOffset StartsAt { get; set; } = startsAt;
    public DateTimeOffset? EndsAt { get; set; } = endsAt;
    public string AssignmentType { get; set; } = assignmentType;
    public string Status { get; set; } = status;
    public string CreatedByPrincipal { get; set; } = createdByPrincipal;
}
