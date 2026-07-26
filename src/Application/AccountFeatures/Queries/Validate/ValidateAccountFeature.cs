namespace TrackHub.Manager.Application.AccountFeatures.Queries.Validate;

[AllowCrossAccount("Cross-service feature probe. Router/SyncWorker and TripManagement call it under their global service identity while gating work for whichever account they are currently processing; the answer is a boolean flag, carries no tenant data.")]
public readonly record struct ValidateFeatureEnabledQuery(Guid AccountId, string FeatureKey) : IRequest<bool>;
public class ValidateFeatureEnabledQueryHandler(IAccountFeatureReader reader) : IRequestHandler<ValidateFeatureEnabledQuery, bool>
{
    public async Task<bool> Handle(ValidateFeatureEnabledQuery request, CancellationToken cancellationToken) => await reader.ValidateFeatureEnabledAsync(request.AccountId, request.FeatureKey, cancellationToken);
}
