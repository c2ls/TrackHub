namespace TrackHub.Manager.Application.BackgroundJobs.Commands;

// Write twin of the already-cross-account BackgroundJobRunQueries. The account is nested in
// BackgroundJobRunDto (and is nullable — platform-level runs carry no account at all).
[Authorize(Resource = Resources.BackgroundJobs, Action = Actions.Write)]
[AllowCrossAccount("Cross-service background-job ledger. Geofencing (geofence_client) and TripManagement (trip_client) have no BackgroundJobRun table of their own and record their job runs here under a global service identity, per account processed; Geofencing's dwell evaluator additionally records platform-level runs with a null account.")]
public readonly record struct CreateBackgroundJobRunCommand(BackgroundJobRunDto BackgroundJobRun) : IRequest<BackgroundJobRunVm>;
public class CreateBackgroundJobRunCommandHandler(IBackgroundJobRunWriter writer) : IRequestHandler<CreateBackgroundJobRunCommand, BackgroundJobRunVm>
{
    public async Task<BackgroundJobRunVm> Handle(CreateBackgroundJobRunCommand request, CancellationToken cancellationToken) => await writer.CreateBackgroundJobRunAsync(request.BackgroundJobRun, cancellationToken);
}
