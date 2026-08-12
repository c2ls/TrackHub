using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TrackHub.Manager.Application.GpsIntegration.Commands;

[Authorize(Resource = Resources.SynchronizedDevices, Action = Actions.Execute)]

// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct TriggerOperatorDeviceSyncCommand(Guid OperatorId, bool ResetDeviceCatalog = false, bool? AutoAssignNewDevices = null) : IRequest<bool>;

public class TriggerOperatorDeviceSyncCommandHandler(
    IOperatorReader operatorReader,
    IOperatorWriter operatorWriter,
    ISyncDispatcher dispatcher,
    IConfiguration configuration,
    ILogger<TriggerOperatorDeviceSyncCommandHandler> logger)
    : IRequestHandler<TriggerOperatorDeviceSyncCommand, bool>
{
    public async Task<bool> Handle(TriggerOperatorDeviceSyncCommand request, CancellationToken cancellationToken)
    {
        var op = await operatorReader.GetOperatorAsync(request.OperatorId, cancellationToken);

        var minIntervalSeconds = int.TryParse(configuration["GpsIntegration:ManualSyncMinIntervalSeconds"], out var v) ? v : 60;
        if (minIntervalSeconds > 0 && op.LastManualSyncAt is { } last)
        {
            var elapsed = DateTimeOffset.UtcNow - last;
            if (elapsed < TimeSpan.FromSeconds(minIntervalSeconds))
            {
                logger.LogInformation(
                    "Manual sync for operator {OperatorId} throttled; last trigger {Elapsed}s ago (min {Min}s).",
                    request.OperatorId, (int)elapsed.TotalSeconds, minIntervalSeconds);
                throw new Common.Application.Exceptions.TooManyRequestsException(
                    $"Manual sync throttled. Wait {minIntervalSeconds - (int)elapsed.TotalSeconds}s before retrying.")
                { RetryAfterSeconds = minIntervalSeconds - (int)elapsed.TotalSeconds };
            }
        }

        // Stamp BEFORE dispatching so failed syncs are throttled too; the completion path
        // re-stamps on success.
        await operatorWriter.MarkManualSyncTriggeredAsync(request.OperatorId, DateTimeOffset.UtcNow, cancellationToken);

        // The correlation id is born here, at the user-facing entry point, and travels through the
        // Router into the recorded sync run — one id links this log line to the run row.
        var correlationId = Guid.NewGuid().ToString();
        logger.LogInformation(
            "Manual device sync accepted for operator {OperatorId} (account {AccountId}); dispatching to Router. Correlation {CorrelationId}, reset {Reset}.",
            request.OperatorId, op.AccountId, correlationId, request.ResetDeviceCatalog);

        var completed = await dispatcher.DispatchManualSyncAsync(
            op.AccountId,
            request.OperatorId,
            correlationId,
            request.ResetDeviceCatalog,
            request.AutoAssignNewDevices,
            cancellationToken);

        logger.LogInformation(
            "Manual device sync for operator {OperatorId} finished: {Result}. Correlation {CorrelationId}.",
            request.OperatorId, completed ? "completed" : "not completed", correlationId);

        return completed;
    }
}
