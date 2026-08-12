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

using TrackHub.TripManagement.Application.Deliveries.Commands.Create;
using TrackHub.TripManagement.Application.Deliveries.Commands.Delete;
using TrackHub.TripManagement.Application.Deliveries.Commands.Update;
using TrackHub.TripManagement.Application.Deliveries.Commands.UpdateOutcome;

namespace TrackHub.TripManagement.Web.GraphQL.Mutation;

/// <summary>Delivery lines on a stop and their recorded outcomes.</summary>
public partial class Mutation
{
    public async Task<DeliveryVm> CreateDelivery([Service] ISender sender, CreateDeliveryCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<bool> UpdateDelivery([Service] ISender sender, UpdateDeliveryCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return true;
    }

    public async Task<bool> UpdateDeliveryOutcome([Service] ISender sender, UpdateDeliveryOutcomeCommand command, CancellationToken cancellationToken)
        => await sender.Send(command, cancellationToken);

    public async Task<Guid> DeleteDelivery([Service] ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteDeliveryCommand(id), cancellationToken);
        return id;
    }
}
