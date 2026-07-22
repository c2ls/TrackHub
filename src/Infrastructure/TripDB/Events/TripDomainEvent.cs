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

using Common.Infrastructure;

namespace TrackHub.TripManagement.Infrastructure.TripDB.Events;

/// <summary>
/// A trip domain event, dispatched by the shared DispatchDomainEventsInterceptor after a
/// successful save. <see cref="EventType"/> carries one of the
/// <see cref="TripEventTypes"/> literals listed in spec 11 section 10, which are the same
/// literals Manager AlertEventTypes mirrors - keep both catalogs aligned.
/// <para>
/// One event class rather than twenty-four near-identical ones: every trip event has the same
/// shape (account, trip, optional child) and consumers in this slice (audit forwarding, alert
/// emission) dispatch on the type discriminator, not on the CLR type.
/// </para>
/// </summary>
public sealed class TripDomainEvent(string eventType, Guid accountId, Guid tripId, Guid? relatedId = null) : BaseEvent
{
    public string EventType { get; } = eventType;
    public Guid AccountId { get; } = accountId;
    public Guid TripId { get; } = tripId;

    /// <summary>The stop, delivery, POD, assignment, route plan or share the event is about.</summary>
    public Guid? RelatedId { get; } = relatedId;
}
