using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class DriverWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IDriverWriter
{
    public async Task<DriverVm> CreateDriverAsync(DriverDto driver, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(driver.AccountId);
        await RequireTransporterInAccountAsync(driver.DefaultTransporterId, accountId, cancellationToken);
        var entity = new Driver(accountId, driver.Name, driver.Phone, driver.DocumentType, driver.DocumentNumber, driver.Active, driver.EmployeeCode, driver.LicenseNumber, driver.LicenseExpiresAt, driver.DefaultTransporterId);
        await Context.Drivers.AddAsync(entity, cancellationToken);
        AddAuditEvent(entity.AccountId, "CreateDriver", "Driver", entity.DriverId.ToString(), null, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task UpdateDriverAsync(Guid driverId, DriverDto driver, CancellationToken cancellationToken)
    {
        var entity = await GetDriverForWriteAsync(driverId, cancellationToken);
        if (driver.AccountId != entity.AccountId)
        {
            throw new ForbiddenAccessException();
        }

        await RequireTransporterInAccountAsync(driver.DefaultTransporterId, entity.AccountId, cancellationToken);

        Context.Drivers.Attach(entity);
        var oldValues = AuditValues(entity);
        entity.Name = driver.Name;
        entity.Phone = driver.Phone;
        entity.DocumentType = driver.DocumentType;
        entity.DocumentNumber = driver.DocumentNumber;
        entity.Active = driver.Active;
        entity.EmployeeCode = driver.EmployeeCode;
        entity.LicenseNumber = driver.LicenseNumber;
        entity.LicenseExpiresAt = driver.LicenseExpiresAt;
        entity.DefaultTransporterId = driver.DefaultTransporterId;
        AddAuditEvent(entity.AccountId, "UpdateDriver", "Driver", entity.DriverId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateDriverAsync(Guid driverId, CancellationToken cancellationToken)
    {
        var entity = await GetDriverForWriteAsync(driverId, cancellationToken);
        Context.Drivers.Attach(entity);
        var oldValues = AuditValues(entity);
        entity.Active = false;
        AddAuditEvent(entity.AccountId, "DeactivateDriver", "Driver", entity.DriverId.ToString(), oldValues, AuditValues(entity));
        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Spec 09 §5: cross-account transporter references must fail. Without this, a driver in account A
    /// could be pointed at a transporter in account B, and because <c>ValidateDriverAssignment</c>
    /// accepts the default-transporter match, <c>DocumentAccessPolicy</c> would then grant that driver
    /// access to account B's documents. The assignment path always checked this; the default-transporter
    /// scalar did not.
    /// </summary>
    private async Task RequireTransporterInAccountAsync(Guid? transporterId, Guid accountId, CancellationToken cancellationToken)
    {
        if (!transporterId.HasValue)
        {
            return;
        }

        var exists = await Context.Transporters.AnyAsync(x => x.TransporterId == transporterId.Value && x.AccountId == accountId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(nameof(Transporter), transporterId.Value.ToString());
        }
    }

    private async Task<Driver> GetDriverForWriteAsync(Guid driverId, CancellationToken cancellationToken)
    {
        var entity = await Context.Drivers.FirstAsync(x => x.DriverId == driverId, cancellationToken);
        RequireAccountWriteAccess(entity.AccountId);
        return entity;
    }

    private static DriverVm ToVm(Driver x) => new(x.DriverId, x.AccountId, x.Name, x.Phone, x.DocumentType, x.DocumentNumber, x.Active, x.EmployeeCode, x.LicenseNumber, x.LicenseExpiresAt, x.DefaultTransporterId, x.LastModified);

    private static string AuditValues(Driver driver)
        => $$"""{"name":{{Quote(driver.Name)}},"phone":{{Quote(driver.Phone)}},"documentType":{{Quote(driver.DocumentType)}},"documentNumber":{{Quote(driver.DocumentNumber)}},"active":{{(driver.Active ? "true" : "false")}},"employeeCode":{{Quote(driver.EmployeeCode)}},"licenseNumber":{{Quote(driver.LicenseNumber)}},"licenseExpiresAt":{{Quote(driver.LicenseExpiresAt?.ToString("O"))}},"defaultTransporterId":{{Quote(driver.DefaultTransporterId?.ToString())}}}""";
}
