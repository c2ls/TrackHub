using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class AccountSupportGrantWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IAccountSupportGrantWriter
{
    public async Task<AccountSupportGrantVm> CreateAccountSupportGrantAsync(AccountSupportGrantDto accountSupportGrant, CancellationToken cancellationToken)
    {
        var entity = new AccountSupportGrant(RequireAccountWriteAccess(accountSupportGrant.AccountId), accountSupportGrant.SupportUserId, accountSupportGrant.Reason, accountSupportGrant.TicketReference, accountSupportGrant.AccessLevel, accountSupportGrant.StartsAt, accountSupportGrant.EndsAt);
        await Context.AccountSupportGrants.AddAsync(entity, cancellationToken);
        AddAuditEvent(entity.AccountId, "CreateAccountSupportGrant", "AccountSupportGrant", entity.AccountSupportGrantId.ToString(), null, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task ApproveAccountSupportGrantAsync(Guid accountSupportGrantId, string approvedBy, CancellationToken cancellationToken)
    {
        var entity = await Context.AccountSupportGrants.FirstAsync(x => x.AccountSupportGrantId == accountSupportGrantId, cancellationToken);
        RequireAccountWriteAccess(entity.AccountId);

        // Separation of duties: a grant may not be approved by the principal that created it.
        var approverId = Principal.UserId?.ToString() ?? Principal.SubjectId;
        if (!string.IsNullOrEmpty(entity.CreatedBy) && string.Equals(entity.CreatedBy, approverId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenAccessException("A support grant must be approved by a principal other than its creator.");
        }

        Context.AccountSupportGrants.Attach(entity);
        var oldValues = AuditValues(entity);
        entity.ApprovedBy = approvedBy;
        entity.ApprovedAt = DateTimeOffset.UtcNow;
        AddAuditEvent(entity.AccountId, "ApproveAccountSupportGrant", "AccountSupportGrant", entity.AccountSupportGrantId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAccountSupportGrantAsync(Guid accountSupportGrantId, string revokedBy, CancellationToken cancellationToken)
    {
        var entity = await Context.AccountSupportGrants.FirstAsync(x => x.AccountSupportGrantId == accountSupportGrantId, cancellationToken);
        RequireAccountWriteAccess(entity.AccountId);
        Context.AccountSupportGrants.Attach(entity);
        var oldValues = AuditValues(entity);
        entity.RevokedBy = revokedBy;
        entity.RevokedAt = DateTimeOffset.UtcNow;
        AddAuditEvent(entity.AccountId, "RevokeAccountSupportGrant", "AccountSupportGrant", entity.AccountSupportGrantId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    private static AccountSupportGrantVm ToVm(AccountSupportGrant x) 
        => new(x.AccountSupportGrantId, x.AccountId, x.SupportUserId, x.Reason, x.TicketReference, x.ApprovedBy, x.ApprovedAt, x.AccessLevel, x.StartsAt, x.EndsAt, x.RevokedAt, x.RevokedBy, x.LastModified);

    private static string AuditValues(AccountSupportGrant grant) 
        => $$"""{"supportUserId":"{{grant.SupportUserId}}","reason":"{{grant.Reason}}","ticketReference":"{{grant.TicketReference}}","approvedBy":{{Quote(grant.ApprovedBy)}},"approvedAt":{{Quote(grant.ApprovedAt)}},"accessLevel":"{{grant.AccessLevel}}","startsAt":{{Quote(grant.StartsAt)}},"endsAt":{{Quote(grant.EndsAt)}},"revokedAt":{{Quote(grant.RevokedAt)}},"revokedBy":{{Quote(grant.RevokedBy)}}}""";

}
