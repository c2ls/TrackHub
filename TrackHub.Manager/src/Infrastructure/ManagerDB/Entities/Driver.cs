using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class Driver(
    Guid accountId,
    string name,
    string? phone,
    string? documentType,
    string? documentNumber,
    bool active,
    string? employeeCode,
    string? licenseNumber,
    DateOnly? licenseExpiresAt,
    Guid? defaultTransporterId) : BaseAuditableEntity
{
    public Guid DriverId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public string Name { get; set; } = name;
    public string? Phone { get; set; } = phone;
    public string? DocumentType { get; set; } = documentType;
    public string? DocumentNumber { get; set; } = documentNumber;
    public bool Active { get; set; } = active;
    public string? EmployeeCode { get; set; } = employeeCode;
    public string? LicenseNumber { get; set; } = licenseNumber;
    public DateOnly? LicenseExpiresAt { get; set; } = licenseExpiresAt;
    public Guid? DefaultTransporterId { get; set; } = defaultTransporterId;
}
