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

namespace TrackHub.Security.Application.ServiceClientPermissions.Queries;

[Authorize(Resource = Resources.ServiceClients, Action = Actions.Read)]
[AllowCrossAccount("Read twin of the SCP console commands below: the Administrator filters service-client grants by a PARTNER's bound account, never the platform operator's own. The ServiceClients/Read gate restricts this to the platform operator.")]
public readonly record struct GetServiceClientPermissionsQuery(string? ClientId = null, Guid? AccountId = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<ServiceClientPermissionVm>>;
public class GetServiceClientPermissionsQueryHandler(IServiceClientPermissionReader reader) : IRequestHandler<GetServiceClientPermissionsQuery, IReadOnlyCollection<ServiceClientPermissionVm>>
{
    public async Task<IReadOnlyCollection<ServiceClientPermissionVm>> Handle(GetServiceClientPermissionsQuery request, CancellationToken cancellationToken)
        => await reader.GetServiceClientPermissionsAsync(request.ClientId, request.AccountId, request.Skip, request.Take, cancellationToken);
}
