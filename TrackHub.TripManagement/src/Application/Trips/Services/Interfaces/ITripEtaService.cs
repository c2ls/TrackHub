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

namespace TrackHub.TripManagement.Application.Trips.Services.Interfaces;

/// <summary>
/// The two hosted trip jobs' business logic, kept in Application so the Web hosted services stay
/// thin loops. Both are <b>on-work-only</b> recorders (SVD-11): they return how much work they did
/// and only then is a <c>BackgroundJobRun</c> row written.
/// </summary>
public interface ITripEtaService
{
    /// <summary>
    /// Recomputes ETAs for in-progress trips and raises <c>TripDelayed</c> once per stop.
    /// Returns the number of stops whose ETA was refreshed.
    /// </summary>
    Task<int> RefreshEtasAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Raises <c>TripStartDue</c> once for each <c>Created</c> trip inside its account's lead
    /// window. Returns the number of reminders raised. It never changes trip status.
    /// </summary>
    Task<int> RaiseStartRemindersAsync(CancellationToken cancellationToken);
}
