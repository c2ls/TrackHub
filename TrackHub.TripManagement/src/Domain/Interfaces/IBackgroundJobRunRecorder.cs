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
/// Records job runs through Manager under <c>trip_client</c>. Both trip jobs are
/// <b>on-work-only</b> recorders (SVD-11): they write a row when they actually refreshed an ETA
/// or raised a reminder, never once per cycle. An old row for these keys is the healthy steady
/// state and <c>/status</c> must render it neutrally.
/// </summary>
public interface IBackgroundJobRunRecorder
{
    Task RecordAsync(
        string jobKey,
        Guid? accountId,
        string? resourceKey,
        string idempotencyKey,
        string status,
        DateTimeOffset startedAt,
        DateTimeOffset? completedAt,
        string? errorCode,
        string? errorMessage,
        CancellationToken cancellationToken);
}
