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

namespace TrackHub.Security.Application.Policies.GetAll;

[Authorize(Resource = Resources.Permissions, Action = Actions.Read)]
public readonly record struct GetPoliciesQuery() : IRequest<IReadOnlyCollection<PolicyVm>>;

// The GetPoliciesQueryHandler is a class that implements the IRequestHandler interface to handle the GetPoliciesQuery.
public class GetPoliciesQueryHandler(IPolicyReader reader) : IRequestHandler<GetPoliciesQuery, IReadOnlyCollection<PolicyVm>>
{
    // The Handle method is responsible for handling the GetPoliciesQuery and returning the result.
    // It asynchronously calls the GetPoliciesAsync method of the IPolicyReader dependency to retrieve all policies.
    public async Task<IReadOnlyCollection<PolicyVm>> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
        => await reader.GetPoliciesAsync(cancellationToken);

}
