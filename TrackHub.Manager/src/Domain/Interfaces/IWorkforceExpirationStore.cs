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

namespace TrackHub.Manager.Domain.Interfaces;

public readonly record struct ExpiringQualificationVm(
    Guid StaffQualificationId, Guid AccountId, Guid StaffId, string QualificationType, DateOnly ExpiresAt);

public interface IWorkforceExpirationStore
{
    Task<IReadOnlyCollection<ExpiringQualificationVm>> GetExpiringAsync(
        IReadOnlyCollection<Guid> accountIds, DateOnly horizon, CancellationToken cancellationToken);

    Task<bool> JobRunSucceededAsync(string idempotencyKey, CancellationToken cancellationToken);

    /// <summary>
    /// The alert event and its idempotency marker commit together, BEFORE notification fan-out, so
    /// "exactly one alert event per threshold" holds even if delivery then fails.
    /// </summary>
    Task RecordJobRunAsync(Guid accountId, string resourceKey, string idempotencyKey, DateTimeOffset startedAt, CancellationToken cancellationToken);
}
