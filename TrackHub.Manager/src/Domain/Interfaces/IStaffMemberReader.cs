namespace TrackHub.Manager.Domain.Interfaces;

public interface IStaffMemberReader
{
    Task<StaffMemberVm> GetStaffMemberAsync(Guid staffId, CancellationToken cancellationToken);
    Task<StaffMembersPageVm> GetStaffMembersByAccountAsync(Guid accountId, string? search, string? staffType, int skip, int take, CancellationToken cancellationToken);
    Task<StaffMembersPageVm> GetStaffMembersByGroupAsync(long groupId, string? search, int skip, int take, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<StaffLookupVm>> GetStaffLookupAsync(Guid accountId, string? search, IReadOnlyCollection<Guid>? staffIds, int fetchSize, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<StaffAssignmentVm>> GetStaffAssignmentsAsync(Guid staffId, CancellationToken cancellationToken);
    Task<bool> ValidateStaffAssignmentAsync(Guid staffId, string resourceType, string resourceId, CancellationToken cancellationToken);

    /// <summary>Staff self-view; the caller pins <paramref name="staffId"/> from the principal.</summary>
    Task<MyStaffProfileVm> GetMyStaffProfileAsync(Guid staffId, CancellationToken cancellationToken);

    Task<bool> StaffBelongsToAccountAsync(Guid staffId, Guid accountId, CancellationToken cancellationToken);
}
