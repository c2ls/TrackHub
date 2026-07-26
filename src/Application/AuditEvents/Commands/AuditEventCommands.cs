namespace TrackHub.Manager.Application.AuditEvents.Commands;

// The account is nested in AuditEventDto. Reporting reaches this same mutation with the exporting
// USER's token (ReportAuditWriter is not asService) and stays same-tenant on its own; the attribute
// is required for Security's forwarding path below.
[Authorize(Resource = Resources.Audit, Action = Actions.Write)]
[AllowCrossAccount("Central audit sink. Security forwards every security audit event here under the global security_client identity (its only grant), stamped with the account of the user the event is ABOUT — the forwarding token has no account claim of its own.")]
public readonly record struct CreateAuditEventCommand(AuditEventDto AuditEvent) : IRequest<AuditEventVm>;
public class CreateAuditEventCommandHandler(IAuditEventWriter writer) : IRequestHandler<CreateAuditEventCommand, AuditEventVm>
{
    public async Task<AuditEventVm> Handle(CreateAuditEventCommand request, CancellationToken cancellationToken) => await writer.CreateAuditEventAsync(request.AuditEvent, cancellationToken);
}
