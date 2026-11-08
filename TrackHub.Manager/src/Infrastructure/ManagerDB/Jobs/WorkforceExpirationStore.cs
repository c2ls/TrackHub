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

using Common.Domain.Constants;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Jobs;

public sealed class WorkforceExpirationStore(IApplicationDbContext context) : IWorkforceExpirationStore
{
    private const string JobKey = BackgroundJobKeys.WorkforceExpirationScan;

    // Revoked qualifications are already dead — expiry alerting would be noise.
    public async Task<IReadOnlyCollection<ExpiringQualificationVm>> GetExpiringAsync(
        IReadOnlyCollection<Guid> accountIds, DateOnly horizon, CancellationToken cancellationToken)
        => await context.StaffQualifications
            .Where(q => accountIds.Contains(q.AccountId)
                && q.Status != StaffQualificationStatuses.Revoked
                && q.ExpiresAt != null
                && q.ExpiresAt <= horizon)
            .Select(q => new ExpiringQualificationVm(
                q.StaffQualificationId, q.AccountId, q.StaffId, q.QualificationType, q.ExpiresAt!.Value))
            .ToListAsync(cancellationToken);

    public async Task<bool> JobRunSucceededAsync(string idempotencyKey, CancellationToken cancellationToken)
        => await context.BackgroundJobRuns.AnyAsync(
            r => r.JobKey == JobKey && r.IdempotencyKey == idempotencyKey && r.Status == "Succeeded", cancellationToken);

    public async Task RecordJobRunAsync(Guid accountId, string resourceKey, string idempotencyKey, DateTimeOffset startedAt, CancellationToken cancellationToken)
    {
        context.ChangeTracker.Clear();
        context.BackgroundJobRuns.Add(new BackgroundJobRun(
            JobKey, accountId, resourceKey, idempotencyKey, "Succeeded", 1, startedAt)
        {
            CompletedAt = DateTimeOffset.UtcNow,
        });
        await context.SaveChangesAsync(cancellationToken);
    }
}
