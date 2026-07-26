using TrackHub.Manager.Application.PointsOfInterest.Commands.Create;
using TrackHub.Manager.Application.PointsOfInterest.Commands.Delete;
using TrackHub.Manager.Application.PointsOfInterest.Commands.Update;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<PointOfInterestVm> CreatePointOfInterest([Service] ISender sender, CreatePointOfInterestCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> UpdatePointOfInterest([Service] ISender sender, Guid id, UpdatePointOfInterestCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return false;
        await sender.Send(command, cancellationToken);
        return true;
    }

    public async Task<Guid> DeletePointOfInterest([Service] ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeletePointOfInterestCommand(id), cancellationToken);
        return id;
    }
}
