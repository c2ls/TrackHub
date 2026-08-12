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

using Common.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Common.Infrastructure.Interceptors;

/// <summary>
/// Dispatches domain events after a successful save.
/// <para>
/// Events are COLLECTED before the save and PUBLISHED after it. Collecting afterwards (the original
/// behaviour) silently dropped every event raised on a DELETED entity: EF detaches removed entries
/// during <c>SaveChanges</c>, so by the time <c>SavedChanges</c> ran, the change tracker no longer
/// held them. Nothing in the platform raised domain events at the time, so the defect was latent
/// until spec 09 added the first producers — including a <c>DriverQualificationDeleted</c> event that
/// never fired. Do not move collection back into the post-save hook.
/// </para>
/// </summary>
public class DispatchDomainEventsInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    // Events snapshotted from the change tracker before the save, awaiting a successful commit. The
    // interceptor is scoped per DbContext, and each save publishes and clears its own batch.
    private readonly List<BaseEvent> _pending = [];

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CollectDomainEvents(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        CollectDomainEvents(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // Dispatches domain events after the database save completes successfully (synchronous path).
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PublishPendingAsync().GetAwaiter().GetResult();

        return base.SavedChanges(eventData, result);
    }

    // Dispatches domain events after the database save completes successfully (asynchronous path).
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await PublishPendingAsync();

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    // A failed save must not leave its events queued — they would otherwise be published by the next
    // successful save on the same context, reporting changes that were rolled back.
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _pending.Clear();
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _pending.Clear();
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    /// <summary>
    /// Snapshots pending domain events off the tracked entities (including Deleted ones, which are
    /// still tracked at this point) and clears them so a subsequent save cannot re-publish them.
    /// </summary>
    public void CollectDomainEvents(DbContext? context)
    {
        if (context == null) return;

        var entities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        _pending.AddRange(entities.SelectMany(e => e.DomainEvents));
        entities.ForEach(e => e.ClearDomainEvents());
    }

    /// <summary>Publishes and drains everything collected for the save that just committed.</summary>
    public async Task PublishPendingAsync()
    {
        if (_pending.Count == 0) return;

        var domainEvents = _pending.ToList();
        _pending.Clear();

        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent);
    }
}
