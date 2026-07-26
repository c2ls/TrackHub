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

namespace TrackHub.Manager.Domain.Constants;

/// <summary>
/// Platform alert-event catalog. Emitters own raising these; kept in Manager until a
/// second service needs them at compile time.
/// </summary>
public static class AlertEventTypes
{
    public const string GeofenceEntered = nameof(GeofenceEntered);
    public const string GeofenceExited = nameof(GeofenceExited);
    public const string GeofenceDwellExceeded = nameof(GeofenceDwellExceeded);
    public const string CommunicationLoss = nameof(CommunicationLoss);
    public const string GpsCredentialExpiring = nameof(GpsCredentialExpiring);
    public const string GpsOperatorPositionSyncFailed = nameof(GpsOperatorPositionSyncFailed);
    public const string DocumentExpiring = nameof(DocumentExpiring);
    public const string DocumentExpired = nameof(DocumentExpired);
    public const string NotificationDeliveryFailed = nameof(NotificationDeliveryFailed);
    public const string DriverQualificationExpiring = nameof(DriverQualificationExpiring);
    public const string DriverQualificationExpired = nameof(DriverQualificationExpired);

    // Trip management (spec 11 §12), emitted by TrackHub.TripManagement under `trip_client`.
    // Severity is assigned by the emitter, not here: Info for all of these EXCEPT
    // TripDelayed, TripRouteDeviation and TripCancelled, which are Warning.
    public const string TripAssigned = nameof(TripAssigned);
    public const string TripStarted = nameof(TripStarted);
    public const string TripStopArrived = nameof(TripStopArrived);
    public const string TripStopDeparted = nameof(TripStopDeparted);
    public const string TripDelayed = nameof(TripDelayed);
    public const string TripRouteDeviation = nameof(TripRouteDeviation);
    public const string TripPodSubmitted = nameof(TripPodSubmitted);
    public const string TripCompleted = nameof(TripCompleted);
    public const string TripCancelled = nameof(TripCancelled);
    public const string TripStartDue = nameof(TripStartDue);

    public static readonly IReadOnlyCollection<string> All =
    [
        GeofenceEntered, GeofenceExited, GeofenceDwellExceeded, CommunicationLoss,
        GpsCredentialExpiring, GpsOperatorPositionSyncFailed, DocumentExpiring, DocumentExpired,
        NotificationDeliveryFailed, DriverQualificationExpiring, DriverQualificationExpired,
        TripAssigned, TripStarted, TripStopArrived, TripStopDeparted, TripDelayed,
        TripRouteDeviation, TripPodSubmitted, TripCompleted, TripCancelled, TripStartDue
    ];
}
