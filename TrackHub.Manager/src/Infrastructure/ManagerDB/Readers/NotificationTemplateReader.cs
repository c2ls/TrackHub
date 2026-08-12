using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Notifications;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class NotificationTemplateReader(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), INotificationTemplateReader
{
    // The key-addressed built-in defaults surfaced read-only in the templates panel. Localized
    // text never lives in the database: these rows are synthesized from the
    // NotificationDefaultMessages resources at query time. Event-type
    // messages use the generic renderer fallback and have no key-addressed default.
    private static readonly (string TemplateKey, string SubjectKey, string BodyKey)[] BuiltInKeys =
    [
        (NotificationMessageRenderer.TestTemplateKey, NotificationDefaultMessages.TestNotificationSubject, NotificationDefaultMessages.TestNotificationBody),
        (NotificationMessageRenderer.DigestTemplateKey, NotificationDefaultMessages.NotificationDigestSubject, NotificationDefaultMessages.NotificationDigestBody)
    ];

    public async Task<IReadOnlyCollection<NotificationTemplateVm>> GetNotificationTemplatesAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var scopedAccountId = RequireAccountAccess(accountId);

        var overrides = await Context.NotificationTemplates
            .Where(x => x.AccountId == scopedAccountId)
            .Select(x => new NotificationTemplateVm(x.NotificationTemplateId, x.AccountId, x.TemplateKey, x.Channel, x.Locale, x.Subject, x.Body, x.Active, x.LastModified))
            .ToListAsync(cancellationToken);

        var builtIns =
            from entry in BuiltInKeys
            from locale in NotificationLocales.All
            select new NotificationTemplateVm(
                Guid.Empty, null, entry.TemplateKey, NotificationChannels.Email, locale,
                NotificationDefaultMessages.Get(entry.SubjectKey, locale),
                NotificationDefaultMessages.Get(entry.BodyKey, locale),
                true, default);

        // Merged view: account overrides win over the built-in default for the same key.
        var overriddenKeys = overrides.Select(x => (x.TemplateKey, x.Channel, x.Locale)).ToHashSet();
        return overrides
            .Concat(builtIns.Where(x => !overriddenKeys.Contains((x.TemplateKey, x.Channel, x.Locale))))
            .OrderBy(x => x.TemplateKey).ThenBy(x => x.Channel).ThenBy(x => x.Locale)
            .ToList();
    }
}
