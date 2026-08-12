using TrackHub.Manager.Application.Drivers.Commands;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<DriverVm> CreateDriver([Service] ISender sender, CreateDriverCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> UpdateDriver([Service] ISender sender, UpdateDriverCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> DeactivateDriver([Service] ISender sender, DeactivateDriverCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }

    public async Task<DriverQualificationVm> CreateDriverQualification([Service] ISender sender, CreateDriverQualificationCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> UpdateDriverQualification([Service] ISender sender, UpdateDriverQualificationCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    // Delete mutations return the deleted entity's identifier, not a boolean (rules.md "Naming").
    public async Task<Guid> DeleteDriverQualification([Service] ISender sender, DeleteDriverQualificationCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return command.DriverQualificationId; }

    public async Task<DriverTransporterAssignmentVm> AssignDriverToTransporter([Service] ISender sender, AssignDriverToTransporterCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> EndDriverAssignment([Service] ISender sender, EndDriverAssignmentCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
}
