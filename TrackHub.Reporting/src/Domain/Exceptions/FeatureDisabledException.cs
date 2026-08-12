namespace TrackHub.Reporting.Domain.Exceptions;

public sealed class FeatureDisabledException(string featureKey)
    : Exception("This feature is not enabled for your account.")
{
    public string Code => "FEATURE_DISABLED";
    public string FeatureKey { get; } = featureKey;
}
