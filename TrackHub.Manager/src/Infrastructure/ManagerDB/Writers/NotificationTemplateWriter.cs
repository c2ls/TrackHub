using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class NotificationTemplateWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), INotificationTemplateWriter
{
    public async Task<NotificationTemplateVm> CreateNotificationTemplateAsync(NotificationTemplateDto template, CancellationToken cancellationToken)
    {
        RequirePrivileged();
        // Accounts create overrides only; platform defaults are resource-synthesized.
        if (!template.AccountId.HasValue)
        {
            throw new ForbiddenAccessException("Platform default templates are seed data and cannot be created through this surface.");
        }

        var accountId = RequireAccountWriteAccess(template.AccountId.Value);
        var duplicate = await Context.NotificationTemplates.AnyAsync(x =>
            x.AccountId == accountId
            && x.TemplateKey == template.TemplateKey
            && x.Channel == template.Channel
            && x.Locale == template.Locale, cancellationToken);
        if (duplicate)
        {
            throw new ConflictException("A template with the same key, channel, and locale already exists.");
        }

        var entity = new NotificationTemplate(accountId, template.TemplateKey, template.Channel, template.Locale, template.Subject, template.Body, template.Active);
        await Context.NotificationTemplates.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task UpdateNotificationTemplateAsync(Guid notificationTemplateId, NotificationTemplateDto template, CancellationToken cancellationToken)
    {
        var entity = await Context.NotificationTemplates.FirstAsync(x => x.NotificationTemplateId == notificationTemplateId, cancellationToken);
        RequireAccountOverride(entity);
        if (template.AccountId != entity.AccountId)
        {
            throw new ForbiddenAccessException();
        }

        var duplicate = await Context.NotificationTemplates.AnyAsync(x =>
            x.NotificationTemplateId != notificationTemplateId
            && x.AccountId == entity.AccountId
            && x.TemplateKey == template.TemplateKey
            && x.Channel == template.Channel
            && x.Locale == template.Locale, cancellationToken);
        if (duplicate)
        {
            throw new ConflictException("A template with the same key, channel, and locale already exists.");
        }

        Context.NotificationTemplates.Attach(entity);
        entity.TemplateKey = template.TemplateKey;
        entity.Channel = template.Channel;
        entity.Locale = template.Locale;
        entity.Subject = template.Subject;
        entity.Body = template.Body;
        entity.Active = template.Active;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteNotificationTemplateAsync(Guid notificationTemplateId, CancellationToken cancellationToken)
    {
        var entity = await Context.NotificationTemplates.FirstAsync(x => x.NotificationTemplateId == notificationTemplateId, cancellationToken);
        RequireAccountOverride(entity);
        Context.NotificationTemplates.Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    private void RequireAccountOverride(NotificationTemplate entity)
    {
        RequirePrivileged();
        if (!entity.AccountId.HasValue)
        {
            throw new ForbiddenAccessException("Platform default templates are read-only to accounts.");
        }

        RequireAccountWriteAccess(entity.AccountId.Value);
    }

    // Templates are an administrative surface; the Notifications action grants are
    // held by every portal role for the self-service surfaces, so admin-only is enforced here.
    private void RequirePrivileged()
    {
        if (!IsPrivileged)
        {
            throw new ForbiddenAccessException("Only administrators or managers may manage notification templates.");
        }
    }

    private static NotificationTemplateVm ToVm(NotificationTemplate x) => new(x.NotificationTemplateId, x.AccountId, x.TemplateKey, x.Channel, x.Locale, x.Subject, x.Body, x.Active, x.LastModified);
}
