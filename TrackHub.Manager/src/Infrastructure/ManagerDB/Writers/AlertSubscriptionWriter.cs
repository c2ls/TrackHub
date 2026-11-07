using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class AlertSubscriptionWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IAlertSubscriptionWriter
{
    public async Task<AlertSubscriptionVm> CreateAlertSubscriptionAsync(AlertSubscriptionDto subscription, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(subscription.AccountId);
        RequireSelfOrPrivileged(subscription.PrincipalType, subscription.PrincipalId);
        await RequirePrincipalInAccountAsync(accountId, subscription.PrincipalType, subscription.PrincipalId, cancellationToken);
        var contact = await ResolveContactAsync(accountId, subscription, cancellationToken);

        var duplicate = await Context.AlertSubscriptions.AnyAsync(x =>
            x.AccountId == accountId
            && x.PrincipalType == subscription.PrincipalType
            && x.PrincipalId == subscription.PrincipalId
            && x.EventTypeFilter == subscription.EventTypeFilter
            && x.Channel == subscription.Channel, cancellationToken);
        if (duplicate)
        {
            throw new ConflictException("An identical subscription already exists.");
        }

        var entity = new AlertSubscription(accountId, subscription.PrincipalType, subscription.PrincipalId, subscription.EventTypeFilter, subscription.Channel, contact, subscription.Enabled);
        await Context.AlertSubscriptions.AddAsync(entity, cancellationToken);
        AddAuditEvent(entity.AccountId, "CreateAlertSubscription", "AlertSubscription", $"{entity.AlertSubscriptionId}", null, Describe(entity));
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task UpdateAlertSubscriptionAsync(Guid alertSubscriptionId, AlertSubscriptionDto subscription, CancellationToken cancellationToken)
    {
        var entity = await RequireScopedAsync(Context.AlertSubscriptions.AsTracking(), x => x.AlertSubscriptionId == alertSubscriptionId, x => x.AccountId, alertSubscriptionId, forWrite: true, cancellationToken);
        RequireSelfOrPrivileged(entity.PrincipalType, entity.PrincipalId);
        if (subscription.AccountId != entity.AccountId)
        {
            throw new ForbiddenAccessException();
        }

        RequireSelfOrPrivileged(subscription.PrincipalType, subscription.PrincipalId);
        await RequirePrincipalInAccountAsync(entity.AccountId, subscription.PrincipalType, subscription.PrincipalId, cancellationToken);
        var contact = await ResolveContactAsync(entity.AccountId, subscription, cancellationToken);

        var duplicate = await Context.AlertSubscriptions.AnyAsync(x =>
            x.AlertSubscriptionId != alertSubscriptionId
            && x.AccountId == entity.AccountId
            && x.PrincipalType == subscription.PrincipalType
            && x.PrincipalId == subscription.PrincipalId
            && x.EventTypeFilter == subscription.EventTypeFilter
            && x.Channel == subscription.Channel, cancellationToken);
        if (duplicate)
        {
            throw new ConflictException("An identical subscription already exists.");
        }

        var previous = Describe(entity);
        entity.PrincipalType = subscription.PrincipalType;
        entity.PrincipalId = subscription.PrincipalId;
        entity.EventTypeFilter = subscription.EventTypeFilter;
        entity.Channel = subscription.Channel;
        entity.Contact = contact;
        entity.Enabled = subscription.Enabled;
        AddAuditEvent(entity.AccountId, "UpdateAlertSubscription", "AlertSubscription", $"{entity.AlertSubscriptionId}", previous, Describe(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAlertSubscriptionAsync(Guid alertSubscriptionId, CancellationToken cancellationToken)
    {
        var entity = await RequireScopedAsync(Context.AlertSubscriptions.AsTracking(), x => x.AlertSubscriptionId == alertSubscriptionId, x => x.AccountId, alertSubscriptionId, forWrite: true, cancellationToken);
        RequireSelfOrPrivileged(entity.PrincipalType, entity.PrincipalId);
        AddAuditEvent(entity.AccountId, "DeleteAlertSubscription", "AlertSubscription", $"{entity.AlertSubscriptionId}", Describe(entity), null);
        Context.AlertSubscriptions.Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    // Admins manage any account principal; everyone else only themselves.
    private void RequireSelfOrPrivileged(string principalType, Guid principalId)
    {
        if (IsPrivileged)
        {
            return;
        }

        var selfType = Principal.PrincipalType == PrincipalType.Staff ? RecipientPrincipalTypes.Staff : RecipientPrincipalTypes.User;
        var selfId = Principal.PrincipalType == PrincipalType.Staff ? Principal.StaffId : Principal.UserId;
        if (selfId == null || principalId != selfId.Value || !string.Equals(principalType, selfType, StringComparison.Ordinal))
        {
            throw new ForbiddenAccessException("Non-administrators may only manage their own subscriptions.");
        }
    }

    // Cross-account recipients are invalid.
    private async Task RequirePrincipalInAccountAsync(Guid accountId, string principalType, Guid principalId, CancellationToken cancellationToken)
    {
        var belongs = principalType switch
        {
            RecipientPrincipalTypes.User => await Context.Users.AnyAsync(x => x.UserId == principalId && x.AccountId == accountId, cancellationToken),
            RecipientPrincipalTypes.Staff => await Context.StaffMembers.AnyAsync(x => x.StaffId == principalId && x.AccountId == accountId, cancellationToken),
            _ => false
        };
        if (!belongs)
        {
            throw new ForbiddenAccessException($"Subscription principal {principalId} does not belong to account {accountId}.");
        }
    }

    // Staff subscriptions may default Contact from Staff.Phone.
    private async Task<string?> ResolveContactAsync(Guid accountId, AlertSubscriptionDto subscription, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(subscription.Contact))
        {
            return subscription.Contact;
        }

        if (subscription.Channel == NotificationChannels.WhatsApp && subscription.PrincipalType == RecipientPrincipalTypes.Staff)
        {
            return await Context.StaffMembers
                .Where(x => x.StaffId == subscription.PrincipalId && x.AccountId == accountId)
                .Select(x => x.Phone)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return null;
    }

    private static AlertSubscriptionVm ToVm(AlertSubscription x) => new(x.AlertSubscriptionId, x.AccountId, x.PrincipalType, x.PrincipalId, x.EventTypeFilter, x.Channel, x.Contact, x.Enabled, x.LastModified);
    private static string Describe(AlertSubscription subscription)
        => AuditJson.Of(new { subscription.PrincipalType, subscription.PrincipalId, subscription.EventTypeFilter, subscription.Channel, subscription.Enabled });
}
