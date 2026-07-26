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
using TrackHub.Manager.Application.Lookups;

namespace TrackHub.Manager.Application.DeviceTransporter.Queries.GetByOperator;

// Deliberately NOT paged: the caller hands this list to the GPS provider as "which devices to
// fetch", so it needs the set entire. Bounded by UnpagedReadLimits instead, which raises rather than
// returning a short list the provider would silently stop polling.
[Authorize(Resource = Resources.Devices, Action = Actions.Read)]
// Enforcement: the reader/writer this handler delegates to extends AccountScopedDataAccess and
// checks the loaded row's owning account (RequireAccountAccess) or filters on the caller's scope.
[AccountScopeEnforcedInHandler]
public readonly record struct GetDeviceTransporterByUserByOperatorQuery(Guid OperatorId) : IRequest<IReadOnlyCollection<DeviceTransporterVm>>;

public class GetDeviceByUserByOperatorQueryHandler(IDeviceTransporterReader reader, IUser user) : IRequestHandler<GetDeviceTransporterByUserByOperatorQuery, IReadOnlyCollection<DeviceTransporterVm>>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    public async Task<IReadOnlyCollection<DeviceTransporterVm>> Handle(GetDeviceTransporterByUserByOperatorQuery request, CancellationToken cancellationToken)
        => UnpagedReadLimits.EnsureWithinCeiling(
            await reader.GetDeviceTransporterByUserAsync(UserId, request.OperatorId, cancellationToken),
            "deviceTransporterByUserByOperator");

}
