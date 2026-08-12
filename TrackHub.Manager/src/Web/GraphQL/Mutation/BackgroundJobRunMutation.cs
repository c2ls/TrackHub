using TrackHub.Manager.Application.BackgroundJobs.Commands;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<BackgroundJobRunVm> CreateBackgroundJobRun([Service] ISender sender, CreateBackgroundJobRunCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
}
