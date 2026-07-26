using TrackHub.Manager.Application.AccountFeatures.Commands;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<AccountFeatureVm> SetAccountFeature([Service] ISender sender, SetAccountFeatureCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<AccountFeatureVm> SetAccountFeatureMaster([Service] ISender sender, SetAccountFeatureMasterCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> DisableAccountFeature([Service] ISender sender, DisableAccountFeatureCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> UpdateAccountFeatureConfiguration([Service] ISender sender, UpdateAccountFeatureConfigurationCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<int> SeedPublicLinksFeature([Service] ISender sender, CancellationToken cancellationToken) => await sender.Send(new SeedPublicLinksFeatureCommand(), cancellationToken);
}
