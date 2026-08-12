// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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


using TrackHub.Security.Domain.Records;
using TrackHub.Security.Infrastructure.SecurityDB.Interfaces;

namespace TrackHub.Security.Infrastructure.SecurityDB.Writers;

// This class represents a writer for the ResourceActionRole entity in the security database.
public sealed class ResourceActionRoleWriter(IApplicationDbContext context) : IResourceActionRoleWriter
{
    // Creates a new ResourceActionRole asynchronously and returns the created ResourceActionRoleVm.
    // param name="resourceActionRoleDto": The ResourceActionRoleDto object.
    // param name="cancellationToken": The cancellation token.
    // returns: The created ResourceActionRoleVm.
    public async Task<ResourceActionRoleVm> CreateResourceActionRoleAsync(ResourceActionRoleDto resourceActionRoleDto, CancellationToken cancellationToken)
    {
        var resourceActionRole = new ResourceActionRole
        {
            ResourceId = resourceActionRoleDto.ResourceId,
            ActionId = resourceActionRoleDto.ActionId,
            RoleId = resourceActionRoleDto.RoleId
        };

        await context.ResourceActionRole.AddAsync(resourceActionRole, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new ResourceActionRoleVm(
            resourceActionRole.ResourceActionRoleId,
            resourceActionRole.ResourceId,
            resourceActionRole.ActionId,
            resourceActionRole.RoleId);
    }

    // Deletes a ResourceActionRole asynchronously based on the provided resource, actions and role ids.
    // param name="resourceId": The resource id.
    // param name="actionId": The action id.
    // param name="roleId": The role id.
    public async Task DeleteResourceActionRoleAsync(int resourceId, int actionId, int roleId, CancellationToken cancellationToken)
    {
        var resourceActionRole = await context.ResourceActionRole
            .FirstOrDefaultAsync(r => r.ActionId == actionId && r.ResourceId == resourceId && r.RoleId == roleId, cancellationToken)
            ?? throw new NotFoundException(nameof(ResourceActionRole), $"{resourceId}-{actionId}-{roleId}");

        context.ResourceActionRole.Attach(resourceActionRole);
        context.ResourceActionRole.Remove(resourceActionRole);
        await context.SaveChangesAsync(cancellationToken);
    }
}
