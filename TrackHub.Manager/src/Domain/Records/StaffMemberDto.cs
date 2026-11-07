namespace TrackHub.Manager.Domain.Records;

public readonly record struct StaffMemberDto(Guid AccountId, string Name, string? Phone, string? DocumentType, string? DocumentNumber, bool Active, string? EmployeeCode, string? LicenseNumber, DateOnly? LicenseExpiresAt, Guid? DefaultTransporterId, string StaffType,
    uint? ExpectedVersion = null);
