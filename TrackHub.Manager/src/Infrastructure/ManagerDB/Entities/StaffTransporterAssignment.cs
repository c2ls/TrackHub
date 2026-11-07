using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

/// <summary>
/// Time-bounded staff↔transporter assignment history (spec 09 §6). Authoritative operational record;
/// <c>Staff.DefaultTransporterId</c> stays as the pre-selected default (spec 09 §18.2). At most one
/// open assignment may exist per (staff, transporter) pair — enforced by the writer, which returns
/// 409 on overlap. A staff may hold several concurrent assignments to DIFFERENT transporters.
/// </summary>
public sealed class StaffTransporterAssignment(
    Guid accountId,
    Guid staffId,
    Guid transporterId,
    DateTimeOffset startsAt,
    DateTimeOffset? endsAt,
    string assignmentType,
    string status,
    string createdByPrincipal) : BaseAuditableEntity
{
    public Guid StaffTransporterAssignmentId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public Guid StaffId { get; set; } = staffId;
    public Guid TransporterId { get; set; } = transporterId;
    public DateTimeOffset StartsAt { get; set; } = startsAt;
    public DateTimeOffset? EndsAt { get; set; } = endsAt;
    public string AssignmentType { get; set; } = assignmentType;
    public string Status { get; set; } = status;
    public string CreatedByPrincipal { get; set; } = createdByPrincipal;
}
