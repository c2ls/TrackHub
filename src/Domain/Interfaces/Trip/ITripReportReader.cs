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

using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Domain.Interfaces.Trip;

// Reads the TripManagement data the spec 11 §13 reports project. Every call runs under the caller's
// propagated token; TripManagement enforces account scoping, `Trips/Export` and the `trip-management`
// feature gate. All four feeds page transparently up to the 100k defensive row cap.
public interface ITripReportReader
{
    // Pre-checks the `trip-management` billing feature so a disabled account fails with
    // FEATURE_DISABLED (403) rather than a raw GraphQL error — the workforce readers' contract.
    Task EnsureTripManagementFeatureAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReportTripVm>> GetTripsAsync(
        DateTimeOffset? from, DateTimeOffset? to, Guid? transporterId, Guid? driverId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReportTripStopVm>> GetTripStopsAsync(
        DateTimeOffset? from, DateTimeOffset? to, Guid? transporterId, Guid? driverId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReportTripTollVm>> GetTripTollsAsync(
        DateTimeOffset? from, DateTimeOffset? to, Guid? transporterId, Guid? driverId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReportTripPodVm>> GetTripProofsOfDeliveryAsync(
        DateTimeOffset? from, DateTimeOffset? to, Guid? transporterId, Guid? driverId, CancellationToken cancellationToken);
}
