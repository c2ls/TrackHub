using TrackHub.Manager.Application.GeocodingProviders.Commands.Create;
using TrackHub.Manager.Application.GeocodingProviders.Commands.Delete;
using TrackHub.Manager.Application.GeocodingProviders.Commands.SetActive;
using TrackHub.Manager.Application.GeocodingProviders.Commands.Update;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<GeocodingProviderVm> CreateGeocodingProvider([Service] ISender sender, CreateGeocodingProviderCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> UpdateGeocodingProvider([Service] ISender sender, Guid id, UpdateGeocodingProviderCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return false;
        await sender.Send(command, cancellationToken);
        return true;
    }

    public async Task<Guid> DeleteGeocodingProvider([Service] ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteGeocodingProviderCommand(id), cancellationToken);
        return id;
    }

    public async Task<bool> SetActiveGeocodingProvider([Service] ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new SetActiveGeocodingProviderCommand(id), cancellationToken);
        return true;
    }
}
