namespace TrackHub.Manager.Domain.Records;

public readonly record struct DriverDto(Guid AccountId, string Name, string? Phone, string? DocumentType, string? DocumentNumber, bool Active, string? EmployeeCode, string? LicenseNumber, DateOnly? LicenseExpiresAt, Guid? DefaultTransporterId);
