namespace TrackHub.Manager.Domain.Models;

public readonly record struct StaffMemberVm(Guid StaffId, Guid AccountId, string Name, string? Phone, string? DocumentType, string? DocumentNumber, bool Active, string? EmployeeCode, string? LicenseNumber, DateOnly? LicenseExpiresAt, Guid? DefaultTransporterId, string StaffType, DateTimeOffset LastModified,
    uint Version);

/// <summary>
/// The legacy assignment projection consumed by <c>DocumentAccessPolicy</c> and spec 10. Spec 09 kept
/// the original five members and only APPENDED the time-bound fields, so existing consumers are
/// unaffected. <c>StartsAt</c>/<c>EndsAt</c>/<c>AssignmentType</c> are null for the synthesized
/// default-transporter entry — a real assignment row always populates them.
/// </summary>
public readonly record struct StaffAssignmentVm(Guid StaffId, Guid AccountId, string ResourceType, string ResourceId, bool Active, DateTimeOffset? StartsAt = null, DateTimeOffset? EndsAt = null, string? AssignmentType = null);

/// <summary>Picker projection: the two fields a dispatcher needs to choose or name a staff, nothing personal.</summary>
public readonly record struct StaffLookupVm(Guid StaffId, string Name, bool Active);
