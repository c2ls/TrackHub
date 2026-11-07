// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Text.Json;
using Common.Domain.Constants;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Jobs;

public sealed class NotificationDispatchStore(IApplicationDbContext context) : INotificationDispatchStore
{
    private const string JobKey = BackgroundJobKeys.NotificationDispatch;

    public async Task<IReadOnlyCollection<DispatchDeliveryVm>> GetStrandedSendingAsync(
        DateTimeOffset cutoff, int batchSize, CancellationToken cancellationToken)
        => await Project(context.NotificationDeliveries
                .Where(d => d.Status == DeliveryStatuses.Sending && d.LastModified <= cutoff)
                .OrderBy(d => d.LastModified)
                .Take(batchSize))
            .ToListAsync(cancellationToken);

    // Backoff eligibility is derived from LastModified (updated on every save): a delivery that failed
    // attempt N waits backoff(N) before the next try. Manual retries clear Error and are picked up
    // immediately. The ladder is the caller's, passed in as one cutoff per attempt count.
    public async Task<IReadOnlyCollection<DispatchDeliveryVm>> GetEligiblePendingAsync(
        IReadOnlyList<DateTimeOffset> attemptCutoffs, DateTimeOffset now, int batchSize, CancellationToken cancellationToken)
    {
        Guard.Against.Expression(c => c != 4, attemptCutoffs.Count, "The dispatch backoff ladder must have four rungs.");
        var (first, second, third, rest) = (attemptCutoffs[0], attemptCutoffs[1], attemptCutoffs[2], attemptCutoffs[3]);

        // Entitlements are part of the selection, not a post-filter: a held delivery (channel or
        // notifications switched off for its account) stays Pending but never occupies a batch slot,
        // so a backlog of held rows cannot starve every other account's sends.
        var enabled = context.AccountFeatures.Where(f => f.Enabled
            && (f.EffectiveFrom == null || f.EffectiveFrom <= now)
            && (f.EffectiveTo == null || f.EffectiveTo >= now));

        return await Project(context.NotificationDeliveries
                .Where(d => d.Status == DeliveryStatuses.Pending
                    && (d.Attempts == 0
                        || d.Error == null
                        || (d.Attempts == 1 && d.LastModified <= first)
                        || (d.Attempts == 2 && d.LastModified <= second)
                        || (d.Attempts == 3 && d.LastModified <= third)
                        || (d.Attempts >= 4 && d.LastModified <= rest))
                    && enabled.Any(f => f.AccountId == d.AccountId && f.FeatureKey == FeatureKeys.Notifications)
                    && (d.Channel == NotificationChannels.Email
                            ? enabled.Any(f => f.AccountId == d.AccountId && f.FeatureKey == FeatureKeys.NotificationsEmail)
                        : d.Channel == NotificationChannels.WhatsApp
                            ? enabled.Any(f => f.AccountId == d.AccountId && f.FeatureKey == FeatureKeys.NotificationsWhatsApp)
                        : d.Channel == NotificationChannels.Push
                            ? enabled.Any(f => f.AccountId == d.AccountId && f.FeatureKey == FeatureKeys.StaffMobile)
                        : true))
                .OrderBy(d => d.Created)
                .Take(batchSize))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkSendingAsync(Guid notificationDeliveryId, CancellationToken cancellationToken)
    {
        context.ChangeTracker.Clear();

        var delivery = await context.NotificationDeliveries
            .FirstOrDefaultAsync(d => d.NotificationDeliveryId == notificationDeliveryId, cancellationToken);
        if (delivery is null)
        {
            return;
        }

        context.NotificationDeliveries.Attach(delivery);
        delivery.Status = DeliveryStatuses.Sending;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationRuleVm?> GetRuleAsync(Guid notificationRuleId, CancellationToken cancellationToken)
        => await context.NotificationRules
            .Where(r => r.NotificationRuleId == notificationRuleId)
            .Select(r => (NotificationRuleVm?)new NotificationRuleVm(
                r.NotificationRuleId, r.AccountId, r.RuleKey, r.RuleType, r.Enabled, r.TriggerEvent,
                r.RecipientSelector, r.ChannelsJson, r.ThrottlingJson, r.ConfigurationJson, r.LastModified, r.Version))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<AlertEventVm?> GetAlertEventAsync(Guid alertEventId, CancellationToken cancellationToken)
        => await context.AlertEvents
            .Where(e => e.AlertEventId == alertEventId)
            .Select(e => (AlertEventVm?)new AlertEventVm(
                e.AlertEventId, e.AccountId, e.EventType, e.Severity, e.SourceModule, e.ResourceType,
                e.ResourceId, e.Status, e.FirstSeenAt, e.LastSeenAt, e.PayloadJson, e.DeduplicationKey, e.LastModified, null))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task ApplyOutcomeAsync(DeliveryOutcome outcome, CancellationToken cancellationToken)
    {
        context.ChangeTracker.Clear();

        var delivery = await context.NotificationDeliveries
            .FirstOrDefaultAsync(d => d.NotificationDeliveryId == outcome.NotificationDeliveryId, cancellationToken);
        if (delivery is null)
        {
            return;
        }

        context.NotificationDeliveries.Attach(delivery);
        delivery.Status = outcome.Status;
        delivery.Attempts = outcome.Attempts;
        delivery.Error = outcome.Error;
        if (outcome.Status == DeliveryStatuses.Sent)
        {
            delivery.SentAt = DateTimeOffset.UtcNow;
            delivery.ProviderMessageId = outcome.ProviderMessageId;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordJobRunAsync(string resourceKey, string idempotencyKey, DateTimeOffset startedAt, CancellationToken cancellationToken)
    {
        context.BackgroundJobRuns.Add(new BackgroundJobRun(
            JobKey, null, resourceKey, idempotencyKey, "Succeeded", 1, startedAt)
        {
            CompletedAt = DateTimeOffset.UtcNow,
        });
        await context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<DispatchDeliveryVm> Project(IQueryable<NotificationDelivery> query)
        => query.Select(d => new DispatchDeliveryVm(
            d.NotificationDeliveryId, d.AccountId, d.NotificationRuleId, d.AlertEventId,
            d.Channel, d.RecipientPrincipalType, d.Recipient, d.Attempts, d.PayloadJson));
}
