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

namespace TrackHub.TripManagement.Application.Deliveries.Commands.Update;

/// <summary>Edits a delivery in place. Cross-stop moves are rejected by the writer.</summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Edit)]
[RequireFeature(FeatureKeys.TripManagement)]
// Enforcement: the handler derives the caller's own account and passes it to the reader/writer,
// which filters every row on it (TripVisibility is the single visibility resolver - spec 11).
[AccountScopeEnforcedInHandler]
public readonly record struct UpdateDeliveryCommand(Guid DeliveryId, DeliveryDto Delivery) : IRequest;

public sealed class UpdateDeliveryCommandHandler(
    IDeliveryWriter writer,
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<UpdateDeliveryCommand>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task Handle(UpdateDeliveryCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);

        await TripVisibility.ResolveVisibleTripByDeliveryAsync(
            reader, request.DeliveryId, caller.AccountId, scopeUserId, cancellationToken);

        await writer.UpdateDeliveryAsync(request.DeliveryId, caller.AccountId, request.Delivery, cancellationToken);
    }
}

public sealed class UpdateDeliveryValidator : AbstractValidator<UpdateDeliveryCommand>
{
    public UpdateDeliveryValidator()
    {
        RuleFor(v => v.DeliveryId).NotEmpty();
        RuleFor(v => v.Delivery).SetValidator(new Deliveries.Commands.DeliveryDtoValidator());
    }
}
