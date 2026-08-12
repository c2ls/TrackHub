using System.Text.Json;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB;

public abstract class AccountScopedDataAccess(IApplicationDbContext context, ICurrentPrincipal principal)
{
    protected IApplicationDbContext Context { get; } = context;
    protected ICurrentPrincipal Principal { get; } = principal;

    protected bool CanAccessAllAccounts => Principal.PrincipalType == PrincipalType.ServiceClient && !Principal.AccountId.HasValue;

    /// <summary>
    /// Whether the current principal reads account-wide (bypassing group scoping): Administrator or
    /// Manager roles, or a global service client. Plain users are narrowed by group membership.
    /// Mirrors the POI privileged-bypass rule.
    /// </summary>
    protected bool IsPrivileged =>
        CanAccessAllAccounts
        || string.Equals(Principal.Role, Roles.Administrator, StringComparison.OrdinalIgnoreCase)
        || string.Equals(Principal.Role, Roles.Manager, StringComparison.OrdinalIgnoreCase);

    protected Guid RequireAccountAccess(Guid accountId)
        => RequireAccountAccess(accountId, forWrite: false);

    /// <summary>
    /// Account-access check for mutation paths: a support-grant-based access requires a writable
    /// (AccessLevel = Full) grant. Writers call this instead of <see cref="RequireAccountAccess(Guid)"/>.
    /// </summary>
    protected Guid RequireAccountWriteAccess(Guid accountId)
        => RequireAccountAccess(accountId, forWrite: true);

    /// <summary>
    /// Authorizes access to <paramref name="accountId"/>. When <paramref name="forWrite"/> is true and
    /// the only basis for access is an <c>AccountSupportGrant</c>, the grant must permit writes
    /// (AccessLevel = Full); a read-only grant is rejected for mutations.
    /// </summary>
    protected Guid RequireAccountAccess(Guid accountId, bool forWrite)
    {
        if (accountId == Guid.Empty)
        {
            throw new ForbiddenAccessException("Insufficient permissions. Required account access: a non-empty account id.");
        }

        if (CanAccessAllAccounts
            || Principal.AccountId == accountId
            || UserBelongsToAccount(accountId)
            || HasActiveSupportGrant(accountId, forWrite))
        {
            return accountId;
        }

        throw new ForbiddenAccessException($"Insufficient permissions. Required account access: {accountId}.");
    }

    protected Guid? ResolveAccountScope(Guid? requestedAccountId)
    {
        if (CanAccessAllAccounts)
        {
            return requestedAccountId;
        }

        if (!Principal.AccountId.HasValue)
        {
            if (Principal.UserId.HasValue)
            {
                var userAccountId = Context.Users
                    .Where(x => x.UserId == Principal.UserId.Value)
                    .Select(x => (Guid?)x.AccountId)
                    .FirstOrDefault();

                if (userAccountId.HasValue)
                {
                    if (requestedAccountId.HasValue && requestedAccountId.Value != userAccountId.Value)
                    {
                        throw new ForbiddenAccessException($"Insufficient permissions. Required account access: {requestedAccountId.Value}.");
                    }

                    return userAccountId.Value;
                }
            }

            throw new ForbiddenAccessException("Insufficient permissions. Required account access: current principal must include or resolve an account id.");
        }

        if (requestedAccountId.HasValue && requestedAccountId.Value != Principal.AccountId.Value)
        {
            throw new ForbiddenAccessException($"Insufficient permissions. Required account access: {requestedAccountId.Value}.");
        }

        return Principal.AccountId.Value;
    }

    protected void AddAuditEvent(Guid accountId, string action, string resourceType, string resourceId, string? oldValuesJson, string? newValuesJson)
    {
        var actorId = Principal.UserId?.ToString()
            ?? Principal.DriverId?.ToString()
            ?? Principal.ClientId
            ?? Principal.SubjectId
            ?? "unknown";

        Context.AuditEvents.Add(new AuditEvent(
            accountId,
            Principal.PrincipalType.ToString(),
            actorId,
            action,
            resourceType,
            resourceId,
            "Succeeded",
            oldValuesJson,
            newValuesJson,
            null,
            null,
            null,
            Principal.CorrelationId));
    }

    private bool HasActiveSupportGrant(Guid accountId, bool forWrite)
    {
        if (!Principal.UserId.HasValue || !string.Equals(Principal.Role, Roles.Administrator, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        return Context.AccountSupportGrants.Any(x =>
            x.AccountId == accountId
            && x.SupportUserId == Principal.UserId.Value
            && x.ApprovedAt.HasValue
            && x.StartsAt <= now
            && x.EndsAt >= now
            && !x.RevokedAt.HasValue
            // A read-only grant may satisfy reads but never a mutation.
            && (!forWrite || x.AccessLevel == SupportAccessLevels.Full));
    }

    private bool UserBelongsToAccount(Guid accountId)
        => Principal.PrincipalType == PrincipalType.User
           && Principal.UserId.HasValue
           && Context.Users.Any(x => x.UserId == Principal.UserId.Value && x.AccountId == accountId);

    protected static string Quote(DateTimeOffset? value) => value.HasValue ? Quote(value.Value.ToString("O")) : "null";

    /// <summary>
    /// Serializes a value as a JSON string literal for the hand-built audit payloads.
    /// <para>
    /// Uses <see cref="JsonSerializer"/> rather than manual escaping: the previous implementation
    /// escaped only <c>\</c> and <c>"</c>, so any control character — a newline in a free-text field
    /// such as a driver qualification's Notes — produced a raw U+000A inside a JSON string and made
    /// <c>audit_events.newvaluesjson</c> unparseable. Every audit payload in this service flows
    /// through here, so fixing it once fixes them all.
    /// </para>
    /// </summary>
    protected static string Quote(string? value) => value == null ? "null" : JsonSerializer.Serialize(value);
}
