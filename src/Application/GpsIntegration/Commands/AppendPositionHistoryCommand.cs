namespace TrackHub.Telemetry.Application.GpsIntegration.Commands;

[Authorize(Resource = Resources.PositionHistory, Action = Actions.Write, PrincipalTypes = "ServiceClient")]
[RequireFeature(FeatureKeys.GpsPositionHistory, AllowGlobalServiceClient = false)]
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
