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

/// <summary>
/// Validates a trip's optional <c>ServiceOrderId</c> against the trip's account.
/// <para>
/// <b>Why this port exists.</b> Spec 11 §5 lists <c>ServiceOrderId</c> among the cross-account
/// references that must be validated at write time, but spec 12 owns service orders and this
/// module has no service-order store or client. The reference was therefore persisted verbatim on
/// create and update and echoed back through <c>TripVm</c>, <c>TripDetailVm</c> and the
/// <c>trip-summary</c> report with no check of any kind — an unvalidated, possibly cross-account id
/// travelling into report output.
/// </para>
/// <para>
/// Rather than either leaving it unchecked or stripping the field, the call sites are made correct
/// and complete NOW: <c>CreateTripCommand</c> and <c>UpdateTripCommand</c> validate the reference
/// exactly like they validate the transporter, driver and geofence. The default implementation is
/// permissive because there is nothing to ask; the moment spec 12 lands, it registers a real
/// implementation over this same port and every trip write starts enforcing the reference with no
/// change to any handler.
/// </para>
/// </summary>
public interface IServiceOrderValidator
{
    /// <summary>
    /// True when the service order exists and belongs to <paramref name="accountId"/>. Callers pass
    /// only non-null ids; a null <c>ServiceOrderId</c> means "no service order" and is always valid.
    /// </summary>
    Task<bool> ExistsInAccountAsync(Guid serviceOrderId, Guid accountId, CancellationToken cancellationToken);
}
