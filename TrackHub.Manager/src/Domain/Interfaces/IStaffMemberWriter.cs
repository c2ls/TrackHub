namespace TrackHub.Manager.Domain.Interfaces;

public interface IStaffMemberWriter
{
    Task<StaffMemberVm> CreateStaffMemberAsync(StaffMemberDto staff, CancellationToken cancellationToken);
    Task UpdateStaffMemberAsync(Guid staffId, StaffMemberDto staff, CancellationToken cancellationToken);
    Task DeactivateStaffMemberAsync(Guid staffId, CancellationToken cancellationToken);
}
