// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using Common.Application.Interfaces;
using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Events;
using TransporterType = Common.Domain.Enums.TransporterType;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;
// This class represents a writer for the Transporter entity
public sealed class TransporterWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), ITransporterWriter
{
    // Creates a new transporter asynchronously
    // Parameters:
    // - transporterDto: The transporter data transfer object
    // - cancellationToken: The cancellation token
    // Returns:
    // - The created transporter view model
    public async Task<TransporterVm> CreateTransporterAsync(TransporterDto transporterDto, CancellationToken cancellationToken)
    {
        var transporter = new Transporter(
            transporterDto.Name,
            transporterDto.TransporterTypeId,
            transporterDto.AccountId);

        await Context.Transporters.AddAsync(transporter, cancellationToken);
        AddAuditEvent(transporter.AccountId, "CreateTransporter", "Transporter", $"{transporter.TransporterId}", null, Describe(transporter));
        await Context.SaveChangesAsync(cancellationToken);

        return new TransporterVm(
            transporter.TransporterId,
            transporter.Name,
            (TransporterType)transporter.TransporterTypeId,
            transporter.TransporterTypeId, transporter.Version);
    }

    // Updates an existing transporter asynchronously
    // Parameters:
    // - transporterDto: The updated transporter data transfer object
    // - cancellationToken: The cancellation token
    public async Task UpdateTransporterAsync(UpdateTransporterDto transporterDto, CancellationToken cancellationToken)
    {
        var transporter = await Context.Transporters.AsTracking().FirstOrDefaultAsync(t => t.TransporterId == transporterDto.TransporterId && t.RetiredAt == null, cancellationToken)
            ?? throw new NotFoundException(nameof(Transporter), $"{transporterDto.TransporterId}");
        RequireRowAccess(transporter.AccountId, nameof(Transporter), $"{transporterDto.TransporterId}", forWrite: true);
        RowVersion.Expect(Context.Transporters, transporter, transporterDto.ExpectedVersion);

        var previous = Describe(transporter);
        transporter.Name = transporterDto.Name;
        transporter.TransporterTypeId = transporterDto.TransporterTypeId;

        AddAuditEvent(transporter.AccountId, "UpdateTransporter", "Transporter", $"{transporter.TransporterId}", previous, Describe(transporter));
        await Context.SaveChangesAsync(cancellationToken);
    }

    // Retires the unit in one save: its devices are released, its live position dropped, and every
    // record that names it is kept.
    public async Task RetireTransporterAsync(Guid transporterId, CancellationToken cancellationToken)
    {
        var transporter = await Context.Transporters.AsTracking()
            .FirstOrDefaultAsync(t => t.TransporterId == transporterId && t.RetiredAt == null, cancellationToken)
            ?? throw new NotFoundException(nameof(Transporter), $"{transporterId}");
        RequireRowAccess(transporter.AccountId, nameof(Transporter), $"{transporterId}", forWrite: true);

        var now = DateTimeOffset.UtcNow;
        var assignments = await Context.TransporterDeviceAssignments.AsTracking()
            .Include(a => a.Device)
            .Where(a => a.TransporterId == transporterId && a.Status == (int)AssignmentStatus.Active)
            .ToListAsync(cancellationToken);
        foreach (var assignment in assignments)
        {
            assignment.Status = (int)AssignmentStatus.Ended;
            assignment.EffectiveTo = now;
            assignment.AssignmentReason = "Transporter retired";
            assignment.Device.DetectedStatus = (int)DetectedStatus.Available;
        }

        var position = await Context.TransporterPositions.AsTracking()
            .FirstOrDefaultAsync(p => p.TransporterId == transporterId, cancellationToken);
        if (position is not null)
        {
            Context.TransporterPositions.Remove(position);
        }

        await ReleaseStaffAsync(transporterId, now, cancellationToken);

        transporter.RetiredAt = now;
        AddAuditEvent(transporter.AccountId, "RetireTransporter", "Transporter", $"{transporter.TransporterId}", Describe(transporter), null);
        await Context.SaveChangesAsync(cancellationToken);
    }

    // No staff keeps a retired unit: running assignments end now, scheduled ones are cancelled, and it
    // leaves every default.
    private async Task ReleaseStaffAsync(Guid transporterId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var assignments = await Context.StaffTransporterAssignments.AsTracking()
            .Where(a => a.TransporterId == transporterId
                && a.Status == StaffAssignmentStatuses.Active
                && (a.EndsAt == null || a.EndsAt > now))
            .ToListAsync(cancellationToken);
        foreach (var assignment in assignments)
        {
            assignment.EndsAt = assignment.StartsAt > now ? assignment.StartsAt : now;
            assignment.Status = StaffAssignmentStatuses.Ended;
            assignment.AddDomainEvent(new StaffAssignmentEndedEvent(assignment.AccountId, assignment.StaffTransporterAssignmentId, assignment.StaffId, transporterId, assignment.EndsAt.Value));
            AddAuditEvent(assignment.AccountId, "EndStaffAssignment", "StaffTransporterAssignment", $"{assignment.StaffTransporterAssignmentId}", null,
                $$"""{"reason":"Transporter retired","endsAt":{{Quote(assignment.EndsAt.Value.ToString("O"))}}}""");
        }

        var defaults = await Context.StaffMembers.AsTracking()
            .Where(d => d.DefaultTransporterId == transporterId)
            .ToListAsync(cancellationToken);
        foreach (var staff in defaults)
        {
            staff.DefaultTransporterId = null;
        }
    }

    public async Task RestoreTransporterAsync(Guid transporterId, CancellationToken cancellationToken)
    {
        var transporter = await Context.Transporters.AsTracking()
            .FirstOrDefaultAsync(t => t.TransporterId == transporterId && t.RetiredAt != null, cancellationToken)
            ?? throw new NotFoundException(nameof(Transporter), $"{transporterId}");
        RequireRowAccess(transporter.AccountId, nameof(Transporter), $"{transporterId}", forWrite: true);

        transporter.RetiredAt = null;
        AddAuditEvent(transporter.AccountId, "RestoreTransporter", "Transporter", $"{transporter.TransporterId}", null, Describe(transporter));
        await Context.SaveChangesAsync(cancellationToken);
    }

    private static string Describe(Transporter transporter)
        => $$"""{"name":{{AuditJson.Quote(transporter.Name)}},"transporterTypeId":{{transporter.TransporterTypeId}}}""";
}
