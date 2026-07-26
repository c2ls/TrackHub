namespace TrackHub.Manager.Application.PlatformStatus.Commands;

[Authorize(Resource = Resources.Administrative, Action = Actions.Write)]
public readonly record struct CreatePlatformAnnouncementCommand(PlatformAnnouncementDto Announcement) : IRequest<PlatformAnnouncementVm>;
public class CreatePlatformAnnouncementCommandHandler(IPlatformAnnouncementWriter writer) : IRequestHandler<CreatePlatformAnnouncementCommand, PlatformAnnouncementVm>
{
    public async Task<PlatformAnnouncementVm> Handle(CreatePlatformAnnouncementCommand request, CancellationToken cancellationToken)
        => await writer.CreatePlatformAnnouncementAsync(request.Announcement, cancellationToken);
}
public class CreatePlatformAnnouncementCommandValidator : AbstractValidator<CreatePlatformAnnouncementCommand>
{
    public CreatePlatformAnnouncementCommandValidator() => PlatformAnnouncementContractRules.Apply(this, x => x.Announcement);
}

[Authorize(Resource = Resources.Administrative, Action = Actions.Write)]
[PlatformScoped("SVD-10 platform status announcements: platform-owned operational notices, Administrative-gated; no tenant owns a row.")]
public readonly record struct UpdatePlatformAnnouncementCommand(Guid PlatformAnnouncementId, PlatformAnnouncementDto Announcement) : IRequest;
public class UpdatePlatformAnnouncementCommandHandler(IPlatformAnnouncementWriter writer) : IRequestHandler<UpdatePlatformAnnouncementCommand>
{
    public async Task Handle(UpdatePlatformAnnouncementCommand request, CancellationToken cancellationToken)
        => await writer.UpdatePlatformAnnouncementAsync(request.PlatformAnnouncementId, request.Announcement, cancellationToken);
}
public class UpdatePlatformAnnouncementCommandValidator : AbstractValidator<UpdatePlatformAnnouncementCommand>
{
    public UpdatePlatformAnnouncementCommandValidator() => PlatformAnnouncementContractRules.Apply(this, x => x.Announcement);
}

[Authorize(Resource = Resources.Administrative, Action = Actions.Write)]
[PlatformScoped("SVD-10 platform status announcements: platform-owned operational notices, Administrative-gated; no tenant owns a row.")]
public readonly record struct DeletePlatformAnnouncementCommand(Guid PlatformAnnouncementId) : IRequest<Guid>;
public class DeletePlatformAnnouncementCommandHandler(IPlatformAnnouncementWriter writer) : IRequestHandler<DeletePlatformAnnouncementCommand, Guid>
{
    public async Task<Guid> Handle(DeletePlatformAnnouncementCommand request, CancellationToken cancellationToken)
    {
        await writer.DeletePlatformAnnouncementAsync(request.PlatformAnnouncementId, cancellationToken);
        return request.PlatformAnnouncementId;
    }
}
