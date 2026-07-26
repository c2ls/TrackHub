using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Events;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class DriverQualificationWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IDriverQualificationWriter
{
    public async Task<DriverQualificationVm> CreateDriverQualificationAsync(DriverQualificationDto qualification, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(qualification.AccountId);
        var driverName = await RequireDriverInAccountAsync(qualification.DriverId, accountId, cancellationToken);
        await RequireDocumentInAccountAsync(qualification.DocumentId, accountId, cancellationToken);

        var entity = new DriverQualification(accountId, qualification.DriverId, qualification.QualificationType, qualification.Category,
            qualification.Number, qualification.IssuedAt, qualification.ExpiresAt, qualification.IssuingAuthority,
            qualification.Status, qualification.DocumentId, qualification.Notes);

        await Context.DriverQualifications.AddAsync(entity, cancellationToken);
        entity.AddDomainEvent(new DriverQualificationCreatedEvent(accountId, entity.DriverQualificationId, entity.DriverId, entity.QualificationType, entity.ExpiresAt));
        AddAuditEvent(accountId, "CreateDriverQualification", "DriverQualification", entity.DriverQualificationId.ToString(), null, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity, driverName);
    }

    public async Task UpdateDriverQualificationAsync(Guid driverQualificationId, DriverQualificationDto qualification, CancellationToken cancellationToken)
    {
        var entity = await GetForWriteAsync(driverQualificationId, cancellationToken);
        if (qualification.AccountId != entity.AccountId)
        {
            throw new ForbiddenAccessException();
        }

        // Re-pointing a qualification at another driver is legal only within the same account.
        await RequireDriverInAccountAsync(qualification.DriverId, entity.AccountId, cancellationToken);
        await RequireDocumentInAccountAsync(qualification.DocumentId, entity.AccountId, cancellationToken);

        Context.DriverQualifications.Attach(entity);
        var oldValues = AuditValues(entity);
        entity.DriverId = qualification.DriverId;
        entity.QualificationType = qualification.QualificationType;
        entity.Category = qualification.Category;
        entity.Number = qualification.Number;
        entity.IssuedAt = qualification.IssuedAt;
        entity.ExpiresAt = qualification.ExpiresAt;
        entity.IssuingAuthority = qualification.IssuingAuthority;
        entity.Status = qualification.Status;
        entity.DocumentId = qualification.DocumentId;
        entity.Notes = qualification.Notes;
        entity.AddDomainEvent(new DriverQualificationUpdatedEvent(entity.AccountId, entity.DriverQualificationId, entity.DriverId, entity.QualificationType, entity.ExpiresAt));
        AddAuditEvent(entity.AccountId, "UpdateDriverQualification", "DriverQualification", entity.DriverQualificationId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    // Hard delete: the before-image is preserved in the audit event, which is the record of history.
    public async Task DeleteDriverQualificationAsync(Guid driverQualificationId, CancellationToken cancellationToken)
    {
        var entity = await GetForWriteAsync(driverQualificationId, cancellationToken);
        var oldValues = AuditValues(entity);
        entity.AddDomainEvent(new DriverQualificationDeletedEvent(entity.AccountId, entity.DriverQualificationId, entity.DriverId, entity.QualificationType));
        Context.DriverQualifications.Remove(entity);
        AddAuditEvent(entity.AccountId, "DeleteDriverQualification", "DriverQualification", entity.DriverQualificationId.ToString(), oldValues, null);
        await Context.SaveChangesAsync(cancellationToken);
    }

    private async Task<DriverQualification> GetForWriteAsync(Guid driverQualificationId, CancellationToken cancellationToken)
    {
        var entity = await Context.DriverQualifications.FirstOrDefaultAsync(x => x.DriverQualificationId == driverQualificationId, cancellationToken)
            ?? throw new NotFoundException(nameof(DriverQualification), driverQualificationId.ToString());
        RequireAccountWriteAccess(entity.AccountId);
        return entity;
    }

    // Returns the driver name so the create path can build its VM without a second query.
    private async Task<string> RequireDriverInAccountAsync(Guid driverId, Guid accountId, CancellationToken cancellationToken)
        => await Context.Drivers
               .Where(x => x.DriverId == driverId && x.AccountId == accountId)
               .Select(x => x.Name)
               .FirstOrDefaultAsync(cancellationToken)
           // A driver in ANOTHER account is reported as not-found, never as a permission hint.
           ?? throw new NotFoundException(nameof(Driver), driverId.ToString());

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

    private static DriverQualificationVm ToVm(DriverQualification x, string driverName)
        => new(x.DriverQualificationId, x.AccountId, x.DriverId, driverName, x.QualificationType, x.Category, x.Number,
            x.IssuedAt, x.ExpiresAt, x.IssuingAuthority, x.Status, x.DocumentId, x.Notes, x.LastModified);

    private static string AuditValues(DriverQualification qualification)
        => $$"""{"driverId":{{Quote(qualification.DriverId.ToString())}},"qualificationType":{{Quote(qualification.QualificationType)}},"category":{{Quote(qualification.Category)}},"number":{{Quote(qualification.Number)}},"issuedAt":{{Quote(qualification.IssuedAt?.ToString("O"))}},"expiresAt":{{Quote(qualification.ExpiresAt?.ToString("O"))}},"issuingAuthority":{{Quote(qualification.IssuingAuthority)}},"status":{{Quote(qualification.Status)}},"documentId":{{Quote(qualification.DocumentId?.ToString())}}}""";
}
