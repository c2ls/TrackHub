using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Events;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class StaffQualificationWriter(IApplicationDbContext context, ICurrentPrincipal principal, IAlertRecorder alerts) : AccountScopedDataAccess(context, principal), IStaffQualificationWriter
{
    public async Task<StaffQualificationVm> CreateStaffQualificationAsync(StaffQualificationDto qualification, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(qualification.AccountId);
        var staffName = await RequireStaffInAccountAsync(qualification.StaffId, accountId, cancellationToken);
        await RequireDocumentInAccountAsync(qualification.DocumentId, accountId, cancellationToken);

        var entity = new StaffQualification(accountId, qualification.StaffId, qualification.QualificationType, qualification.Category,
            qualification.Number, qualification.IssuedAt, qualification.ExpiresAt, qualification.IssuingAuthority,
            qualification.Status, qualification.DocumentId, qualification.Notes);

        await Context.StaffQualifications.AddAsync(entity, cancellationToken);
        entity.AddDomainEvent(new StaffQualificationCreatedEvent(accountId, entity.StaffQualificationId, entity.StaffId, entity.QualificationType, entity.ExpiresAt));
        AddAuditEvent(accountId, "CreateStaffQualification", "StaffQualification", entity.StaffQualificationId.ToString(), null, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity, staffName);
    }

    public async Task UpdateStaffQualificationAsync(Guid staffQualificationId, StaffQualificationDto qualification, CancellationToken cancellationToken)
    {
        var entity = await GetForWriteAsync(staffQualificationId, cancellationToken);
        if (qualification.AccountId != entity.AccountId)
        {
            throw new ForbiddenAccessException();
        }

        // Re-pointing a qualification at another staff is legal only within the same account.
        await RequireStaffInAccountAsync(qualification.StaffId, entity.AccountId, cancellationToken);
        await RequireDocumentInAccountAsync(qualification.DocumentId, entity.AccountId, cancellationToken);

        var oldValues = AuditValues(entity);
        var renewed = entity.ExpiresAt != qualification.ExpiresAt;
        entity.StaffId = qualification.StaffId;
        entity.QualificationType = qualification.QualificationType;
        entity.Category = qualification.Category;
        entity.Number = qualification.Number;
        entity.IssuedAt = qualification.IssuedAt;
        entity.ExpiresAt = qualification.ExpiresAt;
        entity.IssuingAuthority = qualification.IssuingAuthority;
        entity.Status = qualification.Status;
        entity.DocumentId = qualification.DocumentId;
        entity.Notes = qualification.Notes;
        entity.AddDomainEvent(new StaffQualificationUpdatedEvent(entity.AccountId, entity.StaffQualificationId, entity.StaffId, entity.QualificationType, entity.ExpiresAt));
        AddAuditEvent(entity.AccountId, "UpdateStaffQualification", "StaffQualification", entity.StaffQualificationId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);

        if (renewed)
        {
            await alerts.ResolveOpenAsync(entity.AccountId, "StaffQualification", entity.StaffQualificationId.ToString(),
                [AlertEventTypes.StaffQualificationExpiring, AlertEventTypes.StaffQualificationExpired], cancellationToken);
        }
    }

    // Hard delete: the before-image is preserved in the audit event, which is the record of history.
    public async Task DeleteStaffQualificationAsync(Guid staffQualificationId, CancellationToken cancellationToken)
    {
        var entity = await GetForWriteAsync(staffQualificationId, cancellationToken);
        var oldValues = AuditValues(entity);
        entity.AddDomainEvent(new StaffQualificationDeletedEvent(entity.AccountId, entity.StaffQualificationId, entity.StaffId, entity.QualificationType));
        Context.StaffQualifications.Remove(entity);
        AddAuditEvent(entity.AccountId, "DeleteStaffQualification", "StaffQualification", entity.StaffQualificationId.ToString(), oldValues, null);
        await Context.SaveChangesAsync(cancellationToken);
    }

    private async Task<StaffQualification> GetForWriteAsync(Guid staffQualificationId, CancellationToken cancellationToken)
    {
        var entity = await Context.StaffQualifications
            .AsTracking()
            .FirstOrDefaultAsync(x => x.StaffQualificationId == staffQualificationId, cancellationToken)
            ?? throw new NotFoundException(nameof(StaffQualification), staffQualificationId.ToString());
        RequireRowAccess(entity.AccountId, nameof(StaffQualification), staffQualificationId.ToString(), forWrite: true);
        return entity;
    }

    // Returns the staff name so the create path can build its VM without a second query.
    private async Task<string> RequireStaffInAccountAsync(Guid staffId, Guid accountId, CancellationToken cancellationToken)
        => await Context.StaffMembers
               .Where(x => x.StaffId == staffId && x.AccountId == accountId)
               .Select(x => x.Name)
               .FirstOrDefaultAsync(cancellationToken)
           // A staff in ANOTHER account is reported as not-found, never as a permission hint.
           ?? throw new NotFoundException(nameof(StaffMember), staffId.ToString());

    private async Task RequireDocumentInAccountAsync(Guid? documentId, Guid accountId, CancellationToken cancellationToken)
    {
        if (!documentId.HasValue)
        {
            return;
        }

        var exists = await Context.Documents.AnyAsync(x => x.DocumentId == documentId.Value && x.AccountId == accountId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(nameof(Document), documentId.Value.ToString());
        }
    }

    private static StaffQualificationVm ToVm(StaffQualification x, string staffName)
        => new(x.StaffQualificationId, x.AccountId, x.StaffId, staffName, x.QualificationType, x.Category, x.Number,
            x.IssuedAt, x.ExpiresAt, x.IssuingAuthority, x.Status, x.DocumentId, x.Notes, x.LastModified);

    private static string AuditValues(StaffQualification qualification)
        => $$"""{"staffId":{{Quote(qualification.StaffId.ToString())}},"qualificationType":{{Quote(qualification.QualificationType)}},"category":{{Quote(qualification.Category)}},"number":{{Quote(qualification.Number)}},"issuedAt":{{Quote(qualification.IssuedAt?.ToString("O"))}},"expiresAt":{{Quote(qualification.ExpiresAt?.ToString("O"))}},"issuingAuthority":{{Quote(qualification.IssuingAuthority)}},"status":{{Quote(qualification.Status)}},"documentId":{{Quote(qualification.DocumentId?.ToString())}}}""";
}
