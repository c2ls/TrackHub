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

namespace TrackHub.TripManagement.Application.Deliveries.Commands.Delete;

/// <summary>Removes a delivery line from a stop.</summary>
[Authorize(Resource = Resources.Trips, Action = Actions.Delete)]
[RequireFeature(FeatureKeys.TripManagement)]
public readonly record struct DeleteDeliveryCommand(Guid DeliveryId) : IRequest;

public sealed class DeleteDeliveryCommandHandler(
    IDeliveryWriter writer,
    ITripReader reader,
    IUserReader userReader,
    IUser user) : IRequestHandler<DeleteDeliveryCommand>
{
    private Guid UserId { get; } = TripVisibility.RequireUserId(user);

    public async Task Handle(DeleteDeliveryCommand request, CancellationToken cancellationToken)
    {
        var caller = await userReader.GetUserAsync(UserId, cancellationToken);
        var scopeUserId = TripVisibility.ResolveScopeUserId(user, UserId);

        await TripVisibility.ResolveVisibleTripByDeliveryAsync(
            reader, request.DeliveryId, caller.AccountId, scopeUserId, cancellationToken);

        await writer.DeleteDeliveryAsync(request.DeliveryId, caller.AccountId, cancellationToken);
    }
}

public sealed class DeleteDeliveryValidator : AbstractValidator<DeleteDeliveryCommand>
{
    public DeleteDeliveryValidator()
        => RuleFor(v => v.DeliveryId).NotEmpty();
}
