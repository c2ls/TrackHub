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

public sealed class PolicyReader(IApplicationDbContext context) : IPolicyReader
{

    // Get all policies
    // param cancellationToken: A CancellationToken to observe while waiting for the task to complete.
    // returns: A Task that represents the asynchronous operation.
    public async Task<IReadOnlyCollection<PolicyVm>> GetPoliciesAsync(CancellationToken cancellationToken)
        => await context.Policies
            .Select(p => new PolicyVm(p.PolicyId, p.Name))
            .ToListAsync(cancellationToken);

    // Get all resources for a policy
    // param policyId: The policy identifier.
    // param cancellationToken: A CancellationToken to observe while waiting for the task to complete.
    // returns: List of resources ans their actions for a policy.
    public async Task<PolicyResourceVm> GetResourcesAsync(int policyId, CancellationToken cancellationToken)
        => await context.Policies
            .Where(p => p.PolicyId == policyId)
            .Select(p => new PolicyResourceVm
            {
                PolicyId = p.PolicyId,
                Name = p.Name,
                Resources = context.ResourceActionPolicy
                    .Where(rap => rap.PolicyId == p.PolicyId)
                    .Select(rap => rap.ResourceAction.Resource)
                    .Distinct()
                    .Select(res => new ResourceVm
                    {
                        ResourceId = res.ResourceId,
                        ResourceName = res.ResourceName,
                        Actions = context.ResourceActionPolicy
                            .Where(rap => rap.ResourceId == res.ResourceId && rap.PolicyId == p.PolicyId)
                            .Select(rap => rap.ResourceAction.Action)
                            .Select(a => new ActionVm
                            (
                                a.ActionId,
                                a.ActionName,
                                res.ResourceId
                            )).ToList()
                    }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

}
