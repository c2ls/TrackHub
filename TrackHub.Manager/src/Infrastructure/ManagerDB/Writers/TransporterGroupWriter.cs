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
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB;

namespace TrackHub.Manager.Infrastructure.Writers;

// TransporterGroupWriter class for writing transporter group data
public sealed class TransporterGroupWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), ITransporterGroupWriter
{
    // TransporterGroup carries no account of its own; derive it from the transporter for the audit trail.
    private async Task<Guid> ResolveTransporterAccountAsync(Guid transporterId, CancellationToken cancellationToken)
        => await Context.Transporters
            .Where(t => t.TransporterId == transporterId)
            .Select(t => t.AccountId)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Create a new transporter group asynchronously.
    /// </summary>
    /// <param name="transporterGroupDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The created transporter group view model</returns>
    public async Task<TransporterGroupVm> CreateTransporterGroupAsync(TransporterGroupDto transporterGroupDto, CancellationToken cancellationToken)
    {
        var transporterGroup = new TransporterGroup
        {
            TransporterId = transporterGroupDto.TransporterId,
            GroupId = transporterGroupDto.GroupId
        };

        var accountId = await ResolveTransporterAccountAsync(transporterGroup.TransporterId, cancellationToken);
        RequireAccountWriteAccess(accountId);
        await Context.TransportersGroup.AddAsync(transporterGroup, cancellationToken);
        AddAuditEvent(accountId, "CreateTransporterGroup", "TransporterGroup", $"{transporterGroup.TransporterId}:{transporterGroup.GroupId}", null,
            $$"""{"transporterId":"{{transporterGroup.TransporterId}}","groupId":{{transporterGroup.GroupId}}}""");
        await Context.SaveChangesAsync(cancellationToken);

        return new TransporterGroupVm(
            transporterGroup.TransporterId,
            transporterGroup.GroupId);
    }

    /// <summary>
    /// Delete a transporter group asynchronously.
    /// </summary>
    /// <param name="transporterId"></param>
    /// <param name="groupId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task DeleteTransporterGroupAsync(Guid transporterId, long groupId, CancellationToken cancellationToken)
    {
        var transporterGroup = await Context.TransportersGroup.FindAsync([groupId, transporterId], cancellationToken);

        if (transporterGroup != default)
        {
            var accountId = await ResolveTransporterAccountAsync(transporterId, cancellationToken);
            RequireAccountWriteAccess(accountId);
            Context.TransportersGroup.Attach(transporterGroup);

            AddAuditEvent(accountId, "DeleteTransporterGroup", "TransporterGroup", $"{transporterId}:{groupId}",
                $$"""{"transporterId":"{{transporterId}}","groupId":{{groupId}}}""", null);
            Context.TransportersGroup.Remove(transporterGroup);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Delete all transporter groups that match the given transporterId asynchronously.
    /// </summary>
    /// <param name="transporterId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task DeleteTransporterGroupsAsync(Guid transporterId, CancellationToken cancellationToken)
    {
        var transporterGroups = await Context.TransportersGroup
            .Where(tg => tg.TransporterId == transporterId)
            .ToListAsync(cancellationToken);

        if (transporterGroups.Count != 0)
        {
            var accountId = await ResolveTransporterAccountAsync(transporterId, cancellationToken);
            RequireAccountWriteAccess(accountId);
            Context.TransportersGroup.AttachRange(transporterGroups);

            var groupIds = string.Join(",", transporterGroups.Select(tg => tg.GroupId));
            AddAuditEvent(accountId, "DeleteTransporterGroups", "TransporterGroup", transporterId.ToString(),
                $$"""{"transporterId":"{{transporterId}}","groupIds":[{{groupIds}}]}""", null);
            Context.TransportersGroup.RemoveRange(transporterGroups);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}
