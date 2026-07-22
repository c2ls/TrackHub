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
/// Emits trip alerts to Manager under this service's own <c>trip_client</c> identity, never the
/// caller's token. Every call is best-effort at the call site: a Manager outage logs and must
/// never fail position processing or a lifecycle command (spec 11 §7.4, §12).
/// </summary>
public interface IAlertEmitter
{
    /// <summary>
    /// Records one alert. <paramref name="eventType"/> and <paramref name="severity"/> must match
    /// Manager's <c>AlertEventTypes</c>/<c>AlertSeverities</c> catalogs — these literals travel in
    /// GraphQL variables and are invisible to Layer A contract validation, so each one needs a
    /// Layer B round-trip test (rules.md).
    /// </summary>
    Task EmitAsync(string eventType, string severity, string deduplicationKey, TripAlertDto alert, CancellationToken cancellationToken);
}
