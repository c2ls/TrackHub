using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Manager.Domain.Enums;
using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class TransporterDeviceAssignmentWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), ITransporterDeviceAssignmentWriter
{
    public async Task<TransporterDeviceAssignmentVm> AssignAsync(TransporterDeviceAssignmentDto dto, CancellationToken cancellationToken)
    {
        var scopedAccount = RequireAccountWriteAccess(dto.AccountId);

        var device = await Context.Devices.Include(d => d.Operator)
            .FirstOrDefaultAsync(d => d.DeviceId == dto.DeviceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Device), $"{dto.DeviceId}");
        if (device.AccountId != scopedAccount || device.Operator.AccountId != scopedAccount)
        {
            throw new ForbiddenAccessException();
        }
        var transporter = await Context.Transporters.FirstOrDefaultAsync(t => t.TransporterId == dto.TransporterId, cancellationToken)
            ?? throw new NotFoundException(nameof(Transporter), $"{dto.TransporterId}");
        if (transporter.AccountId != scopedAccount || transporter.AccountId != device.AccountId)
        {
            throw new ForbiddenAccessException();
        }

        var now = DateTimeOffset.UtcNow;
        var actorType = Principal.PrincipalType.ToString();

        var existingActive = await Context.TransporterDeviceAssignments
            .Where(a => a.TransporterId == dto.TransporterId && a.DeviceId == dto.DeviceId && a.Status == (int)AssignmentStatus.Active)
            .ToListAsync(cancellationToken);
        foreach (var prior in existingActive)
        {
            prior.Status = (int)AssignmentStatus.Superseded;
            prior.EffectiveTo = now;
        }

        if (dto.IsPrimary)
        {
            var otherPrimaries = await Context.TransporterDeviceAssignments
                .Where(a => a.TransporterId == dto.TransporterId && a.IsPrimary && a.Status == (int)AssignmentStatus.Active)
                .ToListAsync(cancellationToken);
            foreach (var p in otherPrimaries)
            {
                p.IsPrimary = false;
            }
        }

        var entity = new TransporterDeviceAssignment(
            scopedAccount,
            dto.TransporterId,
            dto.DeviceId,
            now,
            dto.Priority,
            dto.IsPrimary,
            (int)AssignmentStatus.Active,
            dto.AssignmentReason,
            actorType);

        await Context.TransporterDeviceAssignments.AddAsync(entity, cancellationToken);
        device.LastAssignedAt = now;
        if (device.DetectedStatus == (int)DetectedStatus.Available || device.DetectedStatus == (int)DetectedStatus.New)
        {
            device.DetectedStatus = (int)DetectedStatus.Assigned;
        }

        AddAuditEvent(scopedAccount, "AssignDeviceToTransporter", nameof(TransporterDeviceAssignment), entity.TransporterDeviceAssignmentId.ToString(), null,
            $"{{\"transporterId\":\"{dto.TransporterId}\",\"deviceId\":\"{dto.DeviceId}\",\"priority\":{dto.Priority},\"isPrimary\":{dto.IsPrimary.ToString().ToLowerInvariant()}}}");

        await Context.SaveChangesAsync(cancellationToken);

        return new TransporterDeviceAssignmentVm(
            entity.TransporterDeviceAssignmentId, entity.AccountId, entity.TransporterId, entity.DeviceId,
            entity.EffectiveFrom, entity.EffectiveTo, entity.Priority, entity.IsPrimary,
            (AssignmentStatus)entity.Status, entity.AssignmentReason, entity.CreatedByPrincipalType, entity.CreatedBy ?? string.Empty);
    }

    public async Task EndAssignmentAsync(Guid assignmentId, string? reason, CancellationToken cancellationToken)
    {
        var entity = await Context.TransporterDeviceAssignments.Include(a => a.Device)
            .FirstOrDefaultAsync(a => a.TransporterDeviceAssignmentId == assignmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(TransporterDeviceAssignment), $"{assignmentId}");
        RequireAccountWriteAccess(entity.AccountId);
        if (entity.Status != (int)AssignmentStatus.Active)
        {
            return;
        }
        var now = DateTimeOffset.UtcNow;
        entity.Status = (int)AssignmentStatus.Ended;
        entity.EffectiveTo = now;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            entity.AssignmentReason = reason;
        }
        AddAuditEvent(entity.AccountId, "EndDeviceTransporterAssignment", nameof(TransporterDeviceAssignment), entity.TransporterDeviceAssignmentId.ToString(), null,
            $"{{\"reason\":{(reason is null ? "null" : $"\"{reason}\"")}}}");
        await Context.SaveChangesAsync(cancellationToken);
    }
}
