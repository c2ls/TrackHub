using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class TransporterDeviceAssignment(
    Guid accountId,
    Guid transporterId,
    Guid deviceId,
    DateTimeOffset effectiveFrom,
    int priority,
    bool isPrimary,
    int status,
    string? assignmentReason,
    string createdByPrincipalType) : BaseAuditableEntity
{
    private Transporter? _transporter;
    private Device? _device;

    public Guid TransporterDeviceAssignmentId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public Guid TransporterId { get; set; } = transporterId;
    public Guid DeviceId { get; set; } = deviceId;
    public DateTimeOffset EffectiveFrom { get; set; } = effectiveFrom;
    public DateTimeOffset? EffectiveTo { get; set; }
    public int Priority { get; set; } = priority;
    public bool IsPrimary { get; set; } = isPrimary;
    public int Status { get; set; } = status;
    public string? AssignmentReason { get; set; } = assignmentReason;

    /// <summary>
    /// Kind of principal that created the assignment (<see cref="Common.Application.Interfaces.PrincipalType"/>).
    /// The principal's identity itself is carried by the inherited <c>CreatedBy</c> audit column.
    /// </summary>
    public string CreatedByPrincipalType { get; set; } = createdByPrincipalType;

    public Transporter Transporter
    {
        get => _transporter ?? throw new InvalidOperationException("Transporter is not loaded");
        set => _transporter = value;
    }

    public Device Device
    {
        get => _device ?? throw new InvalidOperationException("Device is not loaded");
        set => _device = value;
    }
}
