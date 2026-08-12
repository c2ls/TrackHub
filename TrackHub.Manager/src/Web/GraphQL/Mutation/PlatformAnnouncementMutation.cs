using TrackHub.Manager.Application.PlatformStatus.Commands;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<PlatformAnnouncementVm> CreatePlatformAnnouncement([Service] ISender sender, CreatePlatformAnnouncementCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> UpdatePlatformAnnouncement([Service] ISender sender, UpdatePlatformAnnouncementCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<Guid> DeletePlatformAnnouncement([Service] ISender sender, DeletePlatformAnnouncementCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
}
