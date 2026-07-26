namespace TrackHub.Manager.Application.Notifications.Commands;

[Authorize(Resource = Resources.Notifications, Action = Actions.Write)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct CreateNotificationTemplateCommand(NotificationTemplateDto Template) : IRequest<NotificationTemplateVm>;
public class CreateNotificationTemplateCommandHandler(INotificationTemplateWriter writer) : IRequestHandler<CreateNotificationTemplateCommand, NotificationTemplateVm>
{
    public async Task<NotificationTemplateVm> Handle(CreateNotificationTemplateCommand request, CancellationToken cancellationToken) => await writer.CreateNotificationTemplateAsync(request.Template, cancellationToken);
}
public class CreateNotificationTemplateCommandValidator : AbstractValidator<CreateNotificationTemplateCommand>
{
    public CreateNotificationTemplateCommandValidator()
    {
        NotificationTemplateContractRules.Apply(this, x => x.Template);
    }
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.Notifications)]
public readonly record struct UpdateNotificationTemplateCommand(Guid NotificationTemplateId, NotificationTemplateDto Template) : IRequest;
public class UpdateNotificationTemplateCommandHandler(INotificationTemplateWriter writer) : IRequestHandler<UpdateNotificationTemplateCommand>
{
    public async Task Handle(UpdateNotificationTemplateCommand request, CancellationToken cancellationToken) => await writer.UpdateNotificationTemplateAsync(request.NotificationTemplateId, request.Template, cancellationToken);
}
public class UpdateNotificationTemplateCommandValidator : AbstractValidator<UpdateNotificationTemplateCommand>
{
    public UpdateNotificationTemplateCommandValidator()
    {
        NotificationTemplateContractRules.Apply(this, x => x.Template);
    }
}

[Authorize(Resource = Resources.Notifications, Action = Actions.Delete)]
[RequireFeature(FeatureKeys.Notifications)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct DeleteNotificationTemplateCommand(Guid NotificationTemplateId) : IRequest<Guid>;
public class DeleteNotificationTemplateCommandHandler(INotificationTemplateWriter writer) : IRequestHandler<DeleteNotificationTemplateCommand, Guid>
{
    public async Task<Guid> Handle(DeleteNotificationTemplateCommand request, CancellationToken cancellationToken)
    {
        await writer.DeleteNotificationTemplateAsync(request.NotificationTemplateId, cancellationToken);
        return request.NotificationTemplateId;
    }
}
