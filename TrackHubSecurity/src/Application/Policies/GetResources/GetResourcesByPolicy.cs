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

namespace TrackHub.Security.Application.Policies.GetResources;

[Authorize(Resource = Resources.Permissions, Action = Actions.Read)]
public readonly record struct GetResourcesByPolicyQuery(int PolicyId) : IRequest<PolicyResourceVm>;

// The GetResourcesByPolicyQueryHandler class is responsible for handling the GetResourcesByPolicyQuery query.
// It implements the IRequestHandler interface from the MediatR library.
public class GetResourcesByPolicyQueryHandler(IPolicyReader reader) : IRequestHandler<GetResourcesByPolicyQuery, PolicyResourceVm>
{

    // The Handle method is called when the GetResourcesByPolicyQuery query is sent.
    // It returns a PolicyResourceVm object with all the resources for a given policy and their respective actions.
    public async Task<PolicyResourceVm> Handle(GetResourcesByPolicyQuery request, CancellationToken cancellationToken)
        => await reader.GetResourcesAsync(request.PolicyId, cancellationToken);

}
