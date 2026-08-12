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

using TrackHub.Geofencing.Application.GeofenceEvents.Services.Interfaces;

namespace TrackHub.Geofencing.Application.GeofenceEvents.Commands.ProcessPositions;

[Authorize(Resource = Resources.Geofencing, Action = Actions.Custom)]
[AllowCrossAccount("Router/SyncWorker position feed: one global router_client/syncworker_client identity iterates every account and pushes that account's position batch into geofence detection. The token carries no account claim.")]
public readonly record struct ProcessPositionsCommand(
    Guid AccountId,
    IEnumerable<TransporterPositionDto> Positions) : IRequest<GeofenceProcessingResultVm>;

public class ProcessPositionsCommandHandler(IGeofenceDetectionService detectionService, IAccountFeatureReader accountFeatureReader)
    : IRequestHandler<ProcessPositionsCommand, GeofenceProcessingResultVm>
{
    public async Task<GeofenceProcessingResultVm> Handle(
        ProcessPositionsCommand request,
        CancellationToken cancellationToken)
    {
        await accountFeatureReader.EnsureFeatureEnabledAsync(request.AccountId, FeatureKeys.Geofencing, cancellationToken);
        return await detectionService.ProcessPositionsAsync(
            request.Positions,
            request.AccountId,
            cancellationToken);
    }
}

