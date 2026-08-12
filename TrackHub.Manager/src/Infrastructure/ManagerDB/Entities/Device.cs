using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class Device(
    string name,
    int identifier,
    string serial,
    short deviceTypeId,
    string? description,
    string? providerDisplayName,
    string? providerMetadataHash,
    string? providerStatus,
    int detectedStatus,
    Guid operatorId,
    Guid accountId) : BaseAuditableEntity
{
    private Operator? _operator;
    private Account? _account;
    public Guid DeviceId { get; private set; } = Guid.NewGuid();
    public string Name { get; set; } = name;
    public int Identifier { get; set; } = identifier;
    public string Serial { get; set; } = serial;
    public short DeviceTypeId { get; set; } = deviceTypeId;
    public string? Description { get; set; } = description;
    public string? ProviderDisplayName { get; set; } = providerDisplayName;
    public string? ProviderMetadataHash { get; set; } = providerMetadataHash;
    public string? ProviderStatus { get; set; } = providerStatus;
    public int DetectedStatus { get; set; } = detectedStatus;
    public Guid OperatorId { get; set; } = operatorId;
    public Guid AccountId { get; set; } = accountId;

    public DateTimeOffset FirstSeenAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastSyncedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastAssignedAt { get; set; }
    public DateTimeOffset? IgnoredAt { get; set; }

    public ICollection<TransporterDeviceAssignment> Assignments { get; } = [];

    public Operator Operator
    {
        get => _operator ?? throw new InvalidOperationException("Operator is not loaded");
        set => _operator = value;
    }
    public Account Account
    {
        get => _account ?? throw new InvalidOperationException("Account is not loaded");
        set => _account = value;
    }
}
