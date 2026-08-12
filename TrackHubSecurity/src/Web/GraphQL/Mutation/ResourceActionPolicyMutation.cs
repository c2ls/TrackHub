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

using TrackHub.Security.Application.ResourceActionPolicy.Commands.Create;
using TrackHub.Security.Application.ResourceActionPolicy.Commands.Delete;
using TrackHub.Security.Domain.Models;

namespace TrackHub.Security.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<ResourceActionPolicyVm> CreateResourceActionPolicy([Service] ISender sender, CreateResourceActionPolicyCommand command)
        => await sender.Send(command);

    public async Task<int> DeleteResourceActionPolicy([Service] ISender sender, int resourceId, int actionId, int policyId)
    {
        await sender.Send(new DeleteResourceActionPolicyCommand(resourceId, actionId, policyId));
        return policyId;
    }
}
