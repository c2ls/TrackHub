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

public sealed class StaffGroupWriter(IApplicationDbContext context, ICurrentPrincipal principal) : AccountScopedDataAccess(context, principal), IStaffGroupWriter
{
    // Both sides must belong to one account the caller may write; anything else is NotFound.
    // An inactive staff member may leave a group but not join one.
    private async Task<Guid> RequireMembershipAccountAsync(Guid staffId, long groupId, CancellationToken cancellationToken, bool joining = false)
    {
        var staffAccount = await Context.StaffMembers.Where(s => s.StaffId == staffId && (!joining || s.Active))
            .Select(s => (Guid?)s.AccountId).FirstOrDefaultAsync(cancellationToken);
        var groupAccount = await Context.Groups.Where(g => g.GroupId == groupId)
            .Select(g => (Guid?)g.AccountId).FirstOrDefaultAsync(cancellationToken);
        if (staffAccount is not { } accountId || groupAccount != accountId || !HasAccountAccess(accountId, forWrite: true))
        {
            throw new NotFoundException(nameof(StaffGroup), $"{staffId}:{groupId}");
        }

        return accountId;
    }

    public async Task<StaffGroupVm> CreateStaffGroupAsync(StaffGroupDto staffGroupDto, CancellationToken cancellationToken)
    {
        var staffGroup = new StaffGroup { StaffId = staffGroupDto.StaffId, GroupId = staffGroupDto.GroupId };

        var accountId = await RequireMembershipAccountAsync(staffGroup.StaffId, staffGroup.GroupId, cancellationToken, joining: true);
        await Context.StaffGroups.AddAsync(staffGroup, cancellationToken);
        AddAuditEvent(accountId, "CreateStaffGroup", "StaffGroup", $"{staffGroup.StaffId}:{staffGroup.GroupId}", null,
            $$"""{"staffId":"{{staffGroup.StaffId}}","groupId":{{staffGroup.GroupId}}}""");
        await Context.SaveChangesAsync(cancellationToken);

        return new StaffGroupVm(staffGroup.StaffId, staffGroup.GroupId);
    }

    public async Task DeleteStaffGroupAsync(Guid staffId, long groupId, CancellationToken cancellationToken)
    {
        var staffGroup = await Context.StaffGroups.FindAsync([groupId, staffId], cancellationToken);
        if (staffGroup == default)
        {
            return;
        }

        var accountId = await RequireMembershipAccountAsync(staffId, groupId, cancellationToken);
        Context.StaffGroups.Attach(staffGroup);
        AddAuditEvent(accountId, "DeleteStaffGroup", "StaffGroup", $"{staffId}:{groupId}",
            $$"""{"staffId":"{{staffId}}","groupId":{{groupId}}}""", null);
        Context.StaffGroups.Remove(staffGroup);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
