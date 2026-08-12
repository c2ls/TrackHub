using TrackHub.Manager.Application.GpsIntegration.Queries;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<CredentialMetadataVm?> GetOperatorCredentialMetadata([Service] ISender sender, [AsParameters] GetOperatorCredentialMetadataQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);
}
