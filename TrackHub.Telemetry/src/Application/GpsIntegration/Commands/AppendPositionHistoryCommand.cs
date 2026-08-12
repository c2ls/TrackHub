namespace TrackHub.Telemetry.Application.GpsIntegration.Commands;

// Single-row twin of AppendPositionHistoryBatchCommand and subject to the same reasoning. Note this
// command ALREADY projected the nested account to the root (the AccountId accessor below, added so
// FeatureFlagBehavior could see it), which means TrackHubCommon 1.0.6's guard could already see it
// too — without the opt-out it has been failing closed for its only possible caller since 1.0.6.
[Authorize(Resource = Resources.PositionHistory, Action = Actions.Write, PrincipalTypes = "ServiceClient")]
[RequireFeature(FeatureKeys.GpsPositionHistory, AllowGlobalServiceClient = false)]
[AllowCrossAccount("Router/SyncWorker position feed: one global router_client/syncworker_client identity writes history rows for whichever account owns the transporter. The token carries no account claim, so there is nothing to compare the row's AccountId against.")]
public readonly record struct AppendPositionHistoryCommand(TransporterPositionHistoryDto Position) : IRequest<bool>
{
    // Read-only accessor so FeatureFlagBehavior can evaluate the flag against the row's
    // account; the global service client otherwise has no account and the gate fails closed.
    public Guid AccountId => Position.AccountId;
}

public class AppendPositionHistoryCommandHandler(ITransporterPositionHistoryWriter writer, IPositionRetentionPolicyReader policyReader)
    : IRequestHandler<AppendPositionHistoryCommand, bool>
{
    public async Task<bool> Handle(AppendPositionHistoryCommand request, CancellationToken cancellationToken)
    {
        var policy = await policyReader.GetAsync(request.Position.AccountId, cancellationToken);
        if (!policy.HistoryEnabled)
            return false;
        return await writer.AppendAsync(request.Position, cancellationToken);
    }
}
