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

namespace TrackHub.TripManagement.Domain.Interfaces;

public interface IDeliveryWriter
{
    Task<DeliveryVm> CreateDeliveryAsync(Guid tripStopId, Guid accountId, DeliveryDto delivery, CancellationToken cancellationToken);

    /// <summary>Cross-stop moves are rejected — a delivery belongs to the stop it was created on.</summary>
    Task UpdateDeliveryAsync(Guid deliveryId, Guid accountId, DeliveryDto delivery, CancellationToken cancellationToken);

    /// <summary>
    /// Records the outcome (delivered / partially delivered / rejected). Idempotent on the
    /// caller's event id so spec 10's offline outbox can retry safely.
    /// </summary>
    Task<bool> UpdateDeliveryOutcomeAsync(
        Guid deliveryId,
        Guid accountId,
        string status,
        string? observations,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task DeleteDeliveryAsync(Guid deliveryId, Guid accountId, CancellationToken cancellationToken);

    /// <summary>Bulk-sets a stop's pending deliveries when a POD lands without an explicit outcome.</summary>
    Task MarkStopDeliveriesAsync(Guid tripStopId, Guid accountId, string status, CancellationToken cancellationToken);
}
