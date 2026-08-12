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

using TrackHub.Security.Infrastructure.SecurityDB.Interfaces;

namespace TrackHub.Security.Infrastructure.SecurityDB.Readers;

// This class represents a reader for resource action policies in the security database.
public sealed class ResourceActionPolicyReader(IApplicationDbContext context) : IResourceActionPolicyReader
{
    // Retrieves the names of policies associated with a specific resource and action.
    public async Task<IReadOnlyCollection<string>> GetResourceActionPoliciesAsync(string resource, string action, CancellationToken cancellationToken)
        => await context.ResourceActionPolicy
        .Include(rap => rap.Policy)
        .Include(rap => rap.ResourceAction)
            .ThenInclude(ra => ra.Resource)
        .Include(rap => rap.ResourceAction)
            .ThenInclude(ra => ra.Action)
        .Where(rap => rap.ResourceAction.Resource.ResourceName.Equals(resource) && rap.ResourceAction.Action.ActionName.Equals(action))
        .Select(rap => rap.Policy.Name)
        .ToListAsync(cancellationToken);

    // Retrieves the authorized actions for a collection of policies.
    public async Task<IReadOnlyCollection<ResourceActionVm>> GetPolicyAuthorizedActionsAsync(IReadOnlyCollection<int> policies, CancellationToken cancellationToken)
        => await context.ResourceActionPolicy
            .Include(rap => rap.ResourceAction)
                .ThenInclude(ra => ra.Resource)
            .Include(rap => rap.ResourceAction)
                .ThenInclude(ra => ra.Action)
            .Where(rap => policies.Contains(rap.PolicyId))
            .Select(rap => new ResourceActionVm(
                rap.ResourceAction.ResourceId,
                rap.ResourceAction.Resource.ResourceName,
                rap.ResourceAction.ActionId,
                rap.ResourceAction.Action.ActionName
            ))
            .ToListAsync(cancellationToken);
}
