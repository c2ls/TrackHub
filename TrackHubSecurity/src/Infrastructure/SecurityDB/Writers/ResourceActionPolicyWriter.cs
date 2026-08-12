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

// This class represents a writer for the ResourceActionPolicy entity in the security database.
public sealed class ResourceActionPolicyWriter(IApplicationDbContext context) : IResourceActionPolicyWriter
{
    // Creates a new resource action policy asynchronously.
    // param name="resourceActionPolicyDto": The resource action policy data transfer object.
    // param name="cancellationToken": The cancellation token.
    // returns: The created resource action policy view model.
    public async Task<ResourceActionPolicyVm> CreateResourceActionPolicyAsync(ResourceActionPolicyDto resourceActionPolicyDto, CancellationToken cancellationToken)
    {
        var resourceActionPolicy = new ResourceActionPolicy
        {
            ResourceId = resourceActionPolicyDto.ResourceId,
            ActionId = resourceActionPolicyDto.ActionId,
            PolicyId = resourceActionPolicyDto.PolicyId
        };

        await context.ResourceActionPolicy.AddAsync(resourceActionPolicy, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new ResourceActionPolicyVm(
            resourceActionPolicy.ResourceActionPolicyId,
            resourceActionPolicy.ResourceId,
            resourceActionPolicy.ActionId,
            resourceActionPolicy.PolicyId);
    }

    // Deletes a resource action policy asynchronously.
    // param name="resourceId": The resource ID.
    // param name="actionId": The action ID.
    // param name="policyId": The policy ID.
    public async Task DeleteResourceActionPolicyAsync(int resourceId, int actionId, int policyId, CancellationToken cancellationToken)
    {
        var resourceActionPolicy = await context.ResourceActionPolicy
            .FirstOrDefaultAsync(r => r.ResourceId == resourceId && r.ActionId == actionId && r.PolicyId == policyId, cancellationToken)
            ?? throw new NotFoundException(nameof(ResourceActionPolicy), $"{resourceId}-{actionId}-{policyId}");

        context.ResourceActionPolicy.Attach(resourceActionPolicy);
        context.ResourceActionPolicy.Remove(resourceActionPolicy);
        await context.SaveChangesAsync(cancellationToken);
    }
}
