using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class StaffMemberWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IStaffMemberWriter
{
    public async Task<StaffMemberVm> CreateStaffMemberAsync(StaffMemberDto staff, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(staff.AccountId);
        await RequireTransporterInAccountAsync(staff.DefaultTransporterId, accountId, cancellationToken);

        // One person, one staff record: the same identity document is that staff, and a
        // deactivated one comes back instead of being cloned.
        var documentNumber = NormalizeDocumentNumber(staff.DocumentNumber);
        var twin = documentNumber is null
            ? null
            : await Context.StaffMembers.AsTracking().FirstOrDefaultAsync(x => x.AccountId == accountId && x.DocumentNumber == documentNumber, cancellationToken);
        if (twin is { Active: true })
        {
            throw new ConflictException("A staff with the same document number already exists.");
        }

        if (twin is not null)
        {
            var oldValues = AuditValues(twin);
            Apply(twin, staff with { DocumentNumber = documentNumber, Active = true });
            AddAuditEvent(twin.AccountId, "ReactivateStaffMember", "Staff", twin.StaffId.ToString(), oldValues, AuditValues(twin));
            await Context.SaveChangesAsync(cancellationToken);
            return ToVm(twin);
        }

        var entity = new StaffMember(accountId, staff.Name, staff.Phone, staff.DocumentType, documentNumber, staff.Active, staff.EmployeeCode, staff.LicenseNumber, staff.LicenseExpiresAt, staff.DefaultTransporterId, staff.StaffType);
        await Context.StaffMembers.AddAsync(entity, cancellationToken);
        AddAuditEvent(entity.AccountId, "CreateStaffMember", "Staff", entity.StaffId.ToString(), null, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }


    public async Task UpdateStaffMemberAsync(Guid staffId, StaffMemberDto staff, CancellationToken cancellationToken)
    {
        var entity = await GetStaffForWriteAsync(staffId, cancellationToken);
        if (staff.AccountId != entity.AccountId)
        {
            throw new ForbiddenAccessException();
        }

        await RequireTransporterInAccountAsync(staff.DefaultTransporterId, entity.AccountId, cancellationToken);

        var documentNumber = NormalizeDocumentNumber(staff.DocumentNumber);
        if (documentNumber is not null && await Context.StaffMembers.AnyAsync(
            x => x.AccountId == entity.AccountId && x.DocumentNumber == documentNumber && x.StaffId != staffId, cancellationToken))
        {
            throw new ConflictException("A staff with the same document number already exists.");
        }

        RowVersion.Expect(Context.StaffMembers, entity, staff.ExpectedVersion);
        var oldValues = AuditValues(entity);
        Apply(entity, staff with { DocumentNumber = documentNumber });
        AddAuditEvent(entity.AccountId, "UpdateStaffMember", "Staff", entity.StaffId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateStaffMemberAsync(Guid staffId, CancellationToken cancellationToken)
    {
        var entity = await GetStaffForWriteAsync(staffId, cancellationToken);
        var oldValues = AuditValues(entity);
        entity.Active = false;
        AddAuditEvent(entity.AccountId, "DeactivateStaffMember", "Staff", entity.StaffId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Spec 09 §5: cross-account transporter references must fail. Without this, a staff in account A
    /// could be pointed at a transporter in account B, and because <c>ValidateStaffAssignment</c>
    /// accepts the default-transporter match, <c>DocumentAccessPolicy</c> would then grant that staff
    /// access to account B's documents. The assignment path always checked this; the default-transporter
    /// scalar did not.
    /// </summary>
    private async Task RequireTransporterInAccountAsync(Guid? transporterId, Guid accountId, CancellationToken cancellationToken)
    {
        if (!transporterId.HasValue)
        {
            return;
        }

        var exists = await Context.Transporters.AnyAsync(x => x.TransporterId == transporterId.Value && x.AccountId == accountId && x.RetiredAt == null, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(nameof(Transporter), transporterId.Value.ToString());
        }
    }

    private async Task<StaffMember> GetStaffForWriteAsync(Guid staffId, CancellationToken cancellationToken)
    {
        var entity = await RequireScopedAsync(Context.StaffMembers.AsTracking(), x => x.StaffId == staffId, x => x.AccountId, staffId, forWrite: true, cancellationToken);
        return entity;
    }

    private static string? NormalizeDocumentNumber(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Apply(StaffMember entity, StaffMemberDto staff)
    {
        entity.Name = staff.Name;
        entity.Phone = staff.Phone;
        entity.DocumentType = staff.DocumentType;
        entity.DocumentNumber = staff.DocumentNumber;
        entity.Active = staff.Active;
        entity.EmployeeCode = staff.EmployeeCode;
        entity.LicenseNumber = staff.LicenseNumber;
        entity.LicenseExpiresAt = staff.LicenseExpiresAt;
        entity.DefaultTransporterId = staff.DefaultTransporterId;
        entity.StaffType = staff.StaffType;
    }

    private static StaffMemberVm ToVm(StaffMember x) => new(x.StaffId, x.AccountId, x.Name, x.Phone, x.DocumentType, x.DocumentNumber, x.Active, x.EmployeeCode, x.LicenseNumber, x.LicenseExpiresAt, x.DefaultTransporterId, x.StaffType, x.LastModified, x.Version);

    private static string AuditValues(StaffMember staff)
        => $$"""{"name":{{Quote(staff.Name)}},"phone":{{Quote(staff.Phone)}},"documentType":{{Quote(staff.DocumentType)}},"documentNumber":{{Quote(staff.DocumentNumber)}},"active":{{(staff.Active ? "true" : "false")}},"employeeCode":{{Quote(staff.EmployeeCode)}},"licenseNumber":{{Quote(staff.LicenseNumber)}},"licenseExpiresAt":{{Quote(staff.LicenseExpiresAt?.ToString("O"))}},"defaultTransporterId":{{Quote(staff.DefaultTransporterId?.ToString())}},"staffType":{{Quote(staff.StaffType)}}}""";
}
