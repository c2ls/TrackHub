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
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Web.BackgroundServices;

// Scan-result processing: finds Quarantined documents, runs the AV scanner, and
// transitions Quarantined → Clean (Active) / Infected / Failed. Infected files stay undownloadable and
// raise a deduplicated alert. Runs host-internally against the DB directly (no per-account principal).
// Security job — runs regardless of the `documents` feature.
public sealed class DocumentScanService(
    IServiceScopeFactory scopeFactory,
    ILogger<DocumentScanService> logger) : BackgroundService
{
    private const string JobKey = BackgroundJobKeys.DocumentScan;
    private const int BatchSize = 100;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await Task.Delay(StartupDelay, stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunOnceAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Document scan cycle failed."); }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        List<Document> pending;
        using (var scope = scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            pending = await context.Documents
                .Where(d => d.ScanStatus == DocumentScanStatuses.Quarantined && d.Status != DocumentStatuses.Deleted)
                .OrderBy(d => d.LastModified)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);
        }

        foreach (var document in pending)
        {
            var idempotencyKey = $"scan:{document.DocumentId:N}:{document.CurrentVersion}";
            var startedAt = DateTimeOffset.UtcNow;
            try
            {
                await ScanAsync(document, idempotencyKey, startedAt, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Scan failed for document {DocumentId}.", document.DocumentId);
                await RecordFailureAsync(document.DocumentId, document.AccountId, idempotencyKey, startedAt, ex, cancellationToken);
            }
        }

        if (pending.Count > 0)
        {
            logger.LogInformation("Document scan cycle processed {Count} quarantined document(s).", pending.Count);
        }
    }

    // One scope per document, not one per batch. A failed document leaves its mutations — the document's
    // ScanStatus, the version row, any alert event — tracked on the context; sharing that context with the
    // rest of the batch flushed them under the NEXT document's save, committing changes for an item that
    // never scanned. RecordFailureAsync already opens its own scope to dodge exactly that; disposing the
    // context with the item makes the whole path consistent.
    private async Task ScanAsync(Document document, string idempotencyKey, DateTimeOffset startedAt, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var scanner = scope.ServiceProvider.GetRequiredService<IDocumentScanner>();

        var already = await context.BackgroundJobRuns.AnyAsync(
            r => r.JobKey == JobKey && r.IdempotencyKey == idempotencyKey && r.Status == "Succeeded", cancellationToken);
        if (already)
        {
            return;
        }

        var result = await scanner.ScanAsync(document.StorageKey, cancellationToken);
        context.Documents.Attach(document);
        document.ScanStatus = result;
        if (string.Equals(result, DocumentScanStatuses.Clean, StringComparison.OrdinalIgnoreCase) && document.Status == DocumentStatuses.Uploaded)
        {
            document.Status = DocumentStatuses.Active;
        }

        await SyncCurrentVersionScanAsync(context, document, result, cancellationToken);

        if (string.Equals(result, DocumentScanStatuses.Infected, StringComparison.OrdinalIgnoreCase))
        {
            RaiseInfectedAlert(context, document);
        }

        context.AuditEvents.Add(new AuditEvent(document.AccountId, "System", JobKey, "DocumentScanCompleted", "Document", document.DocumentId.ToString(), "Succeeded", null, $$"""{"scanStatus":"{{result}}"}""", null, null, null, null));
        context.BackgroundJobRuns.Add(new BackgroundJobRun(JobKey, document.AccountId, document.DocumentId.ToString(), idempotencyKey, "Succeeded", 1, startedAt) { CompletedAt = DateTimeOffset.UtcNow });
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SyncCurrentVersionScanAsync(ApplicationDbContext context, Document document, string result, CancellationToken cancellationToken)
    {
        var version = await context.DocumentVersions
            .FirstOrDefaultAsync(v => v.DocumentId == document.DocumentId && v.VersionNumber == document.CurrentVersion, cancellationToken);
        if (version is not null)
        {
            context.DocumentVersions.Attach(version);
            version.ScanStatus = result;
        }
    }

    private static void RaiseInfectedAlert(ApplicationDbContext context, Document document)
    {
        var dedupKey = $"document-infected:{document.DocumentId:N}:{document.CurrentVersion}";
        context.AlertEvents.Add(new AlertEvent(document.AccountId, "DocumentScanFailed", "High", "Documents", "Document", document.DocumentId.ToString(), "Open", $$"""{"reason":"infected","category":"{{document.Category}}"}""", dedupKey));
    }

    private async Task RecordFailureAsync(Guid documentId, Guid accountId, string idempotencyKey, DateTimeOffset startedAt, Exception ex, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.BackgroundJobRuns.Add(new BackgroundJobRun(JobKey, accountId, documentId.ToString(), $"{idempotencyKey}:failed:{startedAt:O}", "Failed", 1, startedAt)
            {
                CompletedAt = DateTimeOffset.UtcNow,
                ErrorCode = ex.GetType().Name,
                ErrorMessage = ex.Message
            });
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception recordEx)
        {
            logger.LogError(recordEx, "Failed to record scan failure for document {DocumentId}.", documentId);
        }
    }
}
