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

using TrackHub.TripManagement.Application.TripEvents.Services.Interfaces;

namespace TrackHub.TripManagement.Application.TripEvents.Commands.ProcessTripPositions;

/// <summary>
/// The service-pipeline surface: Router/SyncWorker pushes positions here, best-effort and isolated,
/// right after the geofence feed. It sits on its own <see cref="Resources.TripTracking"/> resource
/// rather than <see cref="Resources.Trips"/> so a compromised Router identity can never read or
/// mutate trips (spec 11 §18.15).
/// <para>
/// <c>AccountId</c> is carried on the request because the caller is a service client, not a user —
/// there is no user account to resolve from. Authorization is enforced by the resource/action pair
/// plus the seeded <c>router_client</c>/<c>syncworker_client</c> grants.
/// </para>
/// </summary>
[Authorize(Resource = Resources.TripTracking, Action = Actions.Custom)]
[RequireFeature(FeatureKeys.TripManagement)]
[AllowCrossAccount("Router/SyncWorker position feed: one global router_client/syncworker_client identity iterates every account and pushes that account's position batch into trip tracking. The token carries no account claim.")]
public readonly record struct ProcessTripPositionsCommand(
    Guid AccountId,
    IEnumerable<TransporterPositionDto> Positions) : IRequest<TripProcessingResultVm>;

public sealed class ProcessTripPositionsCommandHandler(ITripDetectionService detectionService)
    : IRequestHandler<ProcessTripPositionsCommand, TripProcessingResultVm>
{
    public async Task<TripProcessingResultVm> Handle(ProcessTripPositionsCommand request, CancellationToken cancellationToken)
        => await detectionService.ProcessPositionsAsync(request.Positions, request.AccountId, cancellationToken);
}

public sealed class ProcessTripPositionsValidator : AbstractValidator<ProcessTripPositionsCommand>
{
    public ProcessTripPositionsValidator()
    {
        RuleFor(v => v.AccountId).NotEmpty();
        RuleFor(v => v.Positions).NotNull();
    }
}
