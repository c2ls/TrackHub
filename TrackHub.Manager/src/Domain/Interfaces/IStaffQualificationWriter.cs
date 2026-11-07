namespace TrackHub.Manager.Domain.Interfaces;

public interface IStaffQualificationWriter
{
    Task<StaffQualificationVm> CreateStaffQualificationAsync(StaffQualificationDto qualification, CancellationToken cancellationToken);
    Task UpdateStaffQualificationAsync(Guid staffQualificationId, StaffQualificationDto qualification, CancellationToken cancellationToken);

    /// <summary>Hard delete — the trail lives in the audit events raised by this writer.</summary>
    Task DeleteStaffQualificationAsync(Guid staffQualificationId, CancellationToken cancellationToken);
}
