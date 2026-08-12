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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Web.BackgroundServices;

// Byte-retention cleanup. Deletes the stored bytes for document versions that
// are superseded (VersionNumber < CurrentVersion) or belong to a Voided/Deleted document, once the
// retention window has passed. Metadata rows are retained for audit; the current bytes of an Active
// document are never touched. Runs host-internally against the DB directly.
//
// Retention ENFORCEMENT policy, legal holds, and legal/regulatory export packaging are owned by
// specs/24; this job only reclaims storage and honors the legal-hold hook. This is a security/cleanup
// job, so it runs regardless of the `documents` feature.
public sealed class DocumentRetentionCleanupService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<DocumentRetentionCleanupService> logger) : BackgroundService
{
    private const string JobKey = BackgroundJobKeys.DocumentRetentionCleanup;
    private const int BatchSize = 200;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(10);

    // Default 5-year operational retention; overridable per deployment.
    private int RetentionDays => configuration.GetValue<int?>("DocumentStorage:RetentionDays") ?? 1825;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunOnceAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Document retention cleanup cycle failed."); }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IDocumentStorage>();

        var cutoff = DateTimeOffset.UtcNow.AddDays(-Math.Max(0, RetentionDays));

        // Eligible = not yet purged, superseded by a newer version or belonging to a Voided/Deleted
        // document, and past the retention window measured from the parent's last state change
        // (replace/void/delete time — NOT the version's original creation, which would purge prematurely).
        var eligible = await (
            from v in context.DocumentVersions
            join d in context.Documents on v.DocumentId equals d.DocumentId
            where v.BytesPurgedAt == null
                && d.LastModified <= cutoff
                && (v.VersionNumber < d.CurrentVersion
                    || d.Status == DocumentStatuses.Voided
                    || d.Status == DocumentStatuses.Deleted)
            orderby v.CreatedAt
            select v)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        var purged = 0;
        foreach (var version in eligible)
        {
            // Legal-hold hook. No holds exist until that module ships → nothing is retained here.
            if (await HasLegalHoldAsync(context, version.DocumentId, cancellationToken))
            {
                continue;
            }

            var idempotencyKey = $"retention:{version.DocumentVersionId:N}";
            var startedAt = DateTimeOffset.UtcNow;
            try
            {
                await storage.DeleteAsync(version.StorageKey, cancellationToken); // idempotent: no-op if already gone
                context.DocumentVersions.Attach(version);
                version.BytesPurgedAt = DateTimeOffset.UtcNow;
                context.AuditEvents.Add(new AuditEvent(version.AccountId, "System", JobKey, "DocumentBytesPurged", "DocumentVersion", version.DocumentVersionId.ToString(), "Succeeded", null, $$"""{"documentId":"{{version.DocumentId}}","versionNumber":{{version.VersionNumber}}}""", null, null, null, null));
                context.BackgroundJobRuns.Add(new BackgroundJobRun(JobKey, version.AccountId, version.DocumentId.ToString(), idempotencyKey, "Succeeded", 1, startedAt) { CompletedAt = DateTimeOffset.UtcNow });
                await context.SaveChangesAsync(cancellationToken);
                purged++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Retention cleanup failed for document version {VersionId}.", version.DocumentVersionId);
            }
        }

        if (purged > 0)
        {
            logger.LogInformation("Document retention cleanup purged bytes for {Count} version(s).", purged);
        }
    }

    // Placeholder for a future legal-hold model; no holds exist yet, so nothing is withheld.
    private static Task<bool> HasLegalHoldAsync(ApplicationDbContext context, Guid documentId, CancellationToken cancellationToken)
        => Task.FromResult(false);
}
