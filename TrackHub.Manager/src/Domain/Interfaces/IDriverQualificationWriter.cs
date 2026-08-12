namespace TrackHub.Manager.Domain.Interfaces;

public interface IDriverQualificationWriter
{
    Task<DriverQualificationVm> CreateDriverQualificationAsync(DriverQualificationDto qualification, CancellationToken cancellationToken);
    Task UpdateDriverQualificationAsync(Guid driverQualificationId, DriverQualificationDto qualification, CancellationToken cancellationToken);

    /// <summary>Hard delete — the trail lives in the audit events raised by this writer.</summary>
    Task DeleteDriverQualificationAsync(Guid driverQualificationId, CancellationToken cancellationToken);
}
