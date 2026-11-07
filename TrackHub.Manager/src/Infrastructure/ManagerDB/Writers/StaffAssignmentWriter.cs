using Common.Domain.Time;
using System.Text.Json;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using FluentValidation.Results;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Events;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class StaffAssignmentWriter(IApplicationDbContext context, ICurrentPrincipal principal, IAccountTimeZoneResolver? zones = null) : AccountScopedDataAccess(context, principal), IStaffAssignmentWriter
{
    private readonly IAccountTimeZoneResolver _zones = zones ?? UtcAccountTimeZoneResolver.Instance;
    public async Task<StaffTransporterAssignmentVm> AssignStaffToTransporterAsync(Guid staffId, Guid transporterId, DateTimeOffset startsAt, string assignmentType, CancellationToken cancellationToken)
    {
        var staff = await Context.StaffMembers
            .Where(x => x.StaffId == staffId)
            .Select(x => new { x.AccountId, x.Name, x.Active })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(StaffMember), staffId.ToString());

        var accountId = RequireAccountWriteAccess(staff.AccountId);

        if (!staff.Active)
        {
            throw Invalid(nameof(staffId), "The staff is inactive and cannot be assigned to a transporter.");
        }

        // A transporter outside the caller's account is reported as not-found — never as a hint that
        // some other account owns it.
        var transporterName = await Context.Transporters
            .Where(x => x.TransporterId == transporterId && x.AccountId == accountId && x.RetiredAt == null)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Transporter), transporterId.ToString());

        await RequireNoOverlappingAssignmentAsync(staffId, transporterId, startsAt, cancellationToken);
        await RequireLicenseNotExpiredWhenEnforcedAsync(accountId, staffId, cancellationToken);

        var entity = new StaffTransporterAssignment(accountId, staffId, transporterId, startsAt, null,
            assignmentType, StaffAssignmentStatuses.Active, DescribePrincipal());

        await Context.StaffTransporterAssignments.AddAsync(entity, cancellationToken);
        entity.AddDomainEvent(new StaffAssignedToTransporterEvent(accountId, entity.StaffTransporterAssignmentId, staffId, transporterId, startsAt, assignmentType));
        AddAuditEvent(accountId, "AssignStaffToTransporter", "StaffTransporterAssignment", entity.StaffTransporterAssignmentId.ToString(), null, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);

        return ToVm(entity, staff.Name, transporterName);
    }

    public async Task EndStaffAssignmentAsync(Guid staffTransporterAssignmentId, DateTimeOffset? endsAt, CancellationToken cancellationToken)
    {
        var entity = await Context.StaffTransporterAssignments
            .AsTracking()
            .FirstOrDefaultAsync(x => x.StaffTransporterAssignmentId == staffTransporterAssignmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(StaffTransporterAssignment), staffTransporterAssignmentId.ToString());

        RequireRowAccess(entity.AccountId, nameof(StaffTransporterAssignment), staffTransporterAssignmentId.ToString(), forWrite: true);

        // Ended assignments are immutable (spec 09 §7.1) — a correction is a new assignment, not an edit.
        if (entity.Status != StaffAssignmentStatuses.Active || entity.EndsAt <= DateTimeOffset.UtcNow)
        {
            throw new ConflictException($"Assignment {staffTransporterAssignmentId} has already ended and cannot be modified.");
        }

        var effectiveEnd = endsAt ?? DateTimeOffset.UtcNow;
        if (effectiveEnd < entity.StartsAt)
        {
            throw Invalid(nameof(endsAt), "The assignment end must not precede its start.");
        }

        var oldValues = AuditValues(entity);
        entity.EndsAt = effectiveEnd;
        // A FUTURE end date schedules the close; it must not retire the assignment today. §6 defines an
        // active assignment as "EndsAt null / future", and every active-assignment read already filters
        // on `EndsAt > now` — so leaving Status Active keeps the staff's access alive until the date
        // actually passes. Flipping it immediately would revoke transporter and document access early.
        if (effectiveEnd <= DateTimeOffset.UtcNow)
        {
            entity.Status = StaffAssignmentStatuses.Ended;
        }
        entity.AddDomainEvent(new StaffAssignmentEndedEvent(entity.AccountId, entity.StaffTransporterAssignmentId, entity.StaffId, entity.TransporterId, effectiveEnd));
        AddAuditEvent(entity.AccountId, "EndStaffAssignment", "StaffTransporterAssignment", entity.StaffTransporterAssignmentId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// No two assignments for the same (staff, transporter) pair may overlap in time (spec 09 §6).
    /// <para>
    /// A new assignment is always open-ended, so it occupies <c>[startsAt, ∞)</c> and overlaps an
    /// existing <c>[StartsAt, EndsAt ?? ∞)</c> exactly when that one has not already finished by
    /// <paramref name="startsAt"/>. The comparison is against <paramref name="startsAt"/> and NOT
    /// against "now": a BACKDATED assignment must still be rejected when it reaches back into a
    /// period the pair was already covered for. Assignment history is the join key future
    /// hour-producing modules will aggregate against (§18.7), so an overlap here would double-count.
    /// </para>
    /// <para>
    /// Cancelled assignments never happened and therefore never block; Ended ones did happen and do.
    /// </para>
    /// </summary>
    private async Task RequireNoOverlappingAssignmentAsync(Guid staffId, Guid transporterId, DateTimeOffset startsAt, CancellationToken cancellationToken)
    {
        var overlaps = await Context.StaffTransporterAssignments.AnyAsync(x =>
            x.StaffId == staffId
            && x.TransporterId == transporterId
            && x.Status != StaffAssignmentStatuses.Cancelled
            && (x.EndsAt == null || x.EndsAt > startsAt), cancellationToken);

        if (overlaps)
        {
            throw new ConflictException($"Staff {staffId} already has an assignment to transporter {transporterId} covering {startsAt:O}.");
        }
    }

    /// <summary>
    /// Per-account opt-in (spec 09 §18.6): only when the account's <c>workforce</c> feature row carries
    /// <c>blockAssignmentOnExpiredLicense: true</c> does an expired License qualification block the
    /// assignment. Accounts that never set it are unaffected.
    /// </summary>
    private async Task RequireLicenseNotExpiredWhenEnforcedAsync(Guid accountId, Guid staffId, CancellationToken cancellationToken)
    {
        if (!await IsExpiredLicenseBlockingAsync(accountId, cancellationToken))
        {
            return;
        }

        var today = (await _zones.ResolveAsync(accountId, cancellationToken)).Today();
        var licenses = Context.StaffQualifications.Where(x =>
            x.AccountId == accountId
            && x.StaffId == staffId
            && x.QualificationType == StaffQualificationTypes.License);

        // A staff with NO license record at all is not "expired" — accounts that enable workforce
        // before entering qualifications must not be locked out. Only a staff who HAS licence records
        // and none of them currently valid is blocked.
        if (!await licenses.AnyAsync(cancellationToken))
        {
            return;
        }

        // Superseded licences are kept deliberately (§6 is a history model), so the question is whether
        // a VALID one exists — not whether an expired one does. Asking the latter would permanently
        // block any staff who has ever renewed a licence.
        var hasValidLicense = await licenses.AnyAsync(x =>
            x.Status != StaffQualificationStatuses.Revoked
            && (x.ExpiresAt == null || x.ExpiresAt >= today), cancellationToken);

        if (!hasValidLicense)
        {
            throw Invalid(nameof(staffId), "The staff has no currently valid license qualification, and this account blocks assignment in that case.");
        }
    }

    private async Task<bool> IsExpiredLicenseBlockingAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var configurationJson = await Context.AccountFeatures
            .Where(x => x.AccountId == accountId
                && x.FeatureKey == FeatureKeys.Workforce
                && x.Enabled
                && (x.EffectiveFrom == null || x.EffectiveFrom <= now)
                && (x.EffectiveTo == null || x.EffectiveTo >= now))
            .Select(x => x.ConfigurationJson)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(configurationJson))
        {
            return false;
        }

        // Malformed operator-supplied configuration must not take the assignment path down; an
        // unreadable flag is simply "not enforced" (fail-open is correct here — the strict behavior is
        // the opt-in, so the default must survive bad JSON).
        try
        {
            using var document = JsonDocument.Parse(configurationJson);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty(WorkforceLimits.BlockAssignmentOnExpiredLicenseKey, out var flag)
                && flag.ValueKind == JsonValueKind.True;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private string DescribePrincipal()
    {
        var actorId = Principal.UserId?.ToString()
            ?? Principal.StaffId?.ToString()
            ?? Principal.ClientId
            ?? Principal.SubjectId
            ?? "unknown";
        return $"{Principal.PrincipalType}:{actorId}";
    }

    private static ValidationException Invalid(string propertyName, string message)
        => new([new ValidationFailure(propertyName, message)]);

    private static StaffTransporterAssignmentVm ToVm(StaffTransporterAssignment x, string staffName, string transporterName)
        => new(x.StaffTransporterAssignmentId, x.AccountId, x.StaffId, staffName, x.TransporterId, transporterName,
            x.StartsAt, x.EndsAt, x.AssignmentType, x.Status, x.CreatedByPrincipal, x.LastModified);

    private static string AuditValues(StaffTransporterAssignment assignment)
        => $$"""{"staffId":{{Quote(assignment.StaffId.ToString())}},"transporterId":{{Quote(assignment.TransporterId.ToString())}},"startsAt":{{Quote(assignment.StartsAt)}},"endsAt":{{Quote(assignment.EndsAt)}},"assignmentType":{{Quote(assignment.AssignmentType)}},"status":{{Quote(assignment.Status)}}}""";
}
