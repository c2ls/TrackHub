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

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

/// <summary>
/// Writer class for managing groups.
/// </summary>
public sealed class GroupWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IGroupWriter
{
    /// <summary>
    /// Creates a new group.
    /// </summary>
    /// <param name="groupDto">The group data transfer object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created group view model.</returns>
    public async Task<GroupVm> CreateGroupAsync(GroupDto groupDto, Guid accountId, CancellationToken cancellationToken)
    {
        RequireAccountWriteAccess(accountId);
        var group = new Group(
            groupDto.Name,
            groupDto.Description,
            groupDto.Active,
            accountId);

        await Context.Groups.AddAsync(group, cancellationToken);
        AddAuditEvent(accountId, "CreateGroup", "Group", group.GroupId.ToString(), null, GroupAuditValues(group));
        await Context.SaveChangesAsync(cancellationToken);

        return new GroupVm(
            group.GroupId,
            group.Name,
            group.Description,
            group.Active,
            group.AccountId);
    }

    /// <summary>
    /// Updates an existing group.
    /// </summary>
    /// <param name="groupDto">The updated group data transfer object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task UpdateGroupAsync(UpdateGroupDto groupDto, CancellationToken cancellationToken)
    {
        var group = await Context.Groups.FindAsync([groupDto.GroupId], cancellationToken)
            ?? throw new NotFoundException(nameof(Group), $"{groupDto.GroupId}");

        RequireAccountWriteAccess(group.AccountId);
        Context.Groups.Attach(group);

        var oldValues = GroupAuditValues(group);
        group.Name = groupDto.Name;
        group.Description = groupDto.Description;
        group.Active = groupDto.Active;

        AddAuditEvent(group.AccountId, "UpdateGroup", "Group", group.GroupId.ToString(), oldValues, GroupAuditValues(group));
        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes a group.
    /// </summary>
    /// <param name="groupId">The ID of the group to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task DeleteGroupAsync(long groupId, CancellationToken cancellationToken)
    {
        var group = await Context.Groups.FindAsync([groupId], cancellationToken)
            ?? throw new NotFoundException(nameof(Group), $"{groupId}");

        RequireAccountWriteAccess(group.AccountId);
        Context.Groups.Attach(group);

        AddAuditEvent(group.AccountId, "DeleteGroup", "Group", group.GroupId.ToString(), GroupAuditValues(group), null);
        Context.Groups.Remove(group);
        await Context.SaveChangesAsync(cancellationToken);
    }

    private static string GroupAuditValues(Group group)
        => $$"""{"name":{{Quote(group.Name)}},"description":{{Quote(group.Description)}},"active":{{(group.Active ? "true" : "false")}},"accountId":"{{group.AccountId}}"}""";
}
