namespace TrackHub.Manager.Domain.Interfaces;

/// <summary>
/// Matches a recorded AlertEvent against enabled NotificationRules for the account, applies
/// throttling, resolves recipients (selector + enabled subscriptions), and creates Pending
/// NotificationDelivery rows. Never throws for per-rule failures; the caller
/// treats evaluation as non-blocking for the originating command.
/// </summary>
public interface IAlertRuleEvaluator
{
    /// <returns>The number of deliveries created.</returns>
    Task<int> EvaluateAsync(AlertEventVm alertEvent, CancellationToken cancellationToken);
}
