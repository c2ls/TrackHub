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
using TrackHub.TripManagement.Application.Common;

namespace TrackHub.TripManagement.Application.Deliveries.Commands.Create;

/// <summary>Adds a delivery to a stop. A delivery belongs to the stop it was created on.</summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Write)]
[RequireFeature(FeatureKeys.TripManagement)]
// Enforcement: the handler derives the caller's own account and passes it to the reader/writer,
// which filters every row on it (TripVisibility is the single visibility resolver - spec 11).
[AccountScopeEnforcedInHandler]
public readonly record struct CreateDeliveryCommand(Guid TripStopId, DeliveryDto Delivery) : IRequest<DeliveryVm>;

public sealed class CreateDeliveryCommandHandler(
    IDeliveryWriter writer,
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<CreateDeliveryCommand, DeliveryVm>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task<DeliveryVm> Handle(CreateDeliveryCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);

        // Addressed by stop id: the owning trip must be visible to the caller before a delivery
        // line is attached to it.
        await TripVisibility.ResolveVisibleTripByStopAsync(
            reader, request.TripStopId, caller.AccountId, scopeUserId, cancellationToken);

        return await writer.CreateDeliveryAsync(request.TripStopId, caller.AccountId, request.Delivery, cancellationToken);
    }
}

public sealed class CreateDeliveryValidator : AbstractValidator<CreateDeliveryCommand>
{
    public CreateDeliveryValidator()
    {
        RuleFor(v => v.TripStopId).NotEmpty();
        RuleFor(v => v.Delivery).SetValidator(new DeliveryDtoValidator());
    }
}
