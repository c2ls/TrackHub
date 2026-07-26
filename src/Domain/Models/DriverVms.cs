namespace TrackHub.Manager.Domain.Models;

public readonly record struct DriverVm(Guid DriverId, Guid AccountId, string Name, string? Phone, string? DocumentType, string? DocumentNumber, bool Active, string? EmployeeCode, string? LicenseNumber, DateOnly? LicenseExpiresAt, Guid? DefaultTransporterId, DateTimeOffset LastModified);

/// <summary>
/// The legacy assignment projection consumed by <c>DocumentAccessPolicy</c> and spec 10. Spec 09 kept
/// the original five members and only APPENDED the time-bound fields, so existing consumers are
/// unaffected. <c>StartsAt</c>/<c>EndsAt</c>/<c>AssignmentType</c> are null for the synthesized
/// default-transporter entry — a real assignment row always populates them.
/// </summary>
public readonly record struct DriverAssignmentVm(Guid DriverId, Guid AccountId, string ResourceType, string ResourceId, bool Active, DateTimeOffset? StartsAt = null, DateTimeOffset? EndsAt = null, string? AssignmentType = null);
