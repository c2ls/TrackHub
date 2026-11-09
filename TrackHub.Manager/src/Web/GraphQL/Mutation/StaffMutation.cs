using TrackHub.Manager.Application.Staff.Commands;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<StaffMemberVm> CreateStaffMember([Service] ISender sender, CreateStaffMemberCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> UpdateStaffMember([Service] ISender sender, UpdateStaffMemberCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> DeactivateStaffMember([Service] ISender sender, DeactivateStaffMemberCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<StaffGroupVm> CreateStaffGroup([Service] ISender sender, CreateStaffGroupCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> DeleteStaffGroup([Service] ISender sender, Guid staffId, long groupId, CancellationToken cancellationToken) { await sender.Send(new DeleteStaffGroupCommand(staffId, groupId), cancellationToken); return true; }

    public async Task<StaffQualificationVm> CreateStaffQualification([Service] ISender sender, CreateStaffQualificationCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> UpdateStaffQualification([Service] ISender sender, UpdateStaffQualificationCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    // Delete mutations return the deleted entity's identifier, not a boolean (rules.md "Naming").
    public async Task<Guid> DeleteStaffQualification([Service] ISender sender, DeleteStaffQualificationCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return command.StaffQualificationId; }

    public async Task<StaffTransporterAssignmentVm> AssignStaffToTransporter([Service] ISender sender, AssignStaffToTransporterCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> EndStaffAssignment([Service] ISender sender, EndStaffAssignmentCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
}
