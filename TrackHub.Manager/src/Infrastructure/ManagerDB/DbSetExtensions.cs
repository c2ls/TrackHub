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

using System.Linq.Expressions;

namespace TrackHub.Manager.Infrastructure;

public static class DbSetExtensions
{

    /// <summary>
    /// Bulk adds or updates entities in the DbSet. Optimized for scenarios where most entities already exist.
    /// Fetches all existing entities in a single query and uses dictionary lookups for O(1) matching.
    /// When multiple entities share the same key, only the last occurrence is processed.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="dbSet">The DbSet to add or update the entities in.</param>
    /// <param name="entities">The entities to add or update.</param>
    /// <param name="keySelector">An expression to select the key of the entity.</param>
    /// <param name="excludeProperties">A collection of property names to exclude from the update.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task BulkAddOrUpdateAsync<TEntity, TKey>(
        this DbSet<TEntity> dbSet,
        IEnumerable<TEntity> entities,
        Expression<Func<TEntity, TKey>> keySelector,
        IEnumerable<string> excludeProperties,
        CancellationToken cancellationToken = default)
        where TEntity : class
        where TKey : notnull
    {
        var entityList = entities.ToList();
        if (entityList.Count == 0)
            return;

        // Compile the key selector once for reuse
        var compiledKeySelector = keySelector.Compile();

        // Deduplicate incoming entities by key, keeping the last occurrence
        var deduplicatedEntities = entityList
            .Select((entity, index) => (entity, index, key: compiledKeySelector(entity)))
            .Where(x => x.key != null)
            .GroupBy(x => x.key)
            .Select(g => g.OrderByDescending(x => x.index).First().entity)
            .ToList();

        // Extract all unique keys from the deduplicated entities
        var incomingKeys = deduplicatedEntities
            .Select(compiledKeySelector)
            .ToList();

        // Build a predicate to fetch all existing entities in a single query
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var keySelectorBody = Expression.Invoke(keySelector, parameter);

        // Create: e => incomingKeys.Contains(keySelector(e))
        var containsMethod = typeof(Enumerable)
            .GetMethods()
            .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TKey));

        var containsCall = Expression.Call(
            containsMethod,
            Expression.Constant(incomingKeys),
            keySelectorBody);

        var predicate = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameter);

        // Fetch all existing entities in a single query
        var existingEntities = await dbSet
            .Where(predicate)
            .ToListAsync(cancellationToken);

        // Build a dictionary for O(1) lookups
        var existingDict = existingEntities.ToDictionary(compiledKeySelector);

        // Cache exclude properties as a HashSet for faster lookup
        var excludeSet = excludeProperties.ToHashSet();

        // Cache property infos for the entity type
        var propertyInfoCache = typeof(TEntity)
            .GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .ToDictionary(p => p.Name);

        foreach (var entity in deduplicatedEntities)
        {
            var key = compiledKeySelector(entity);

            if (existingDict.TryGetValue(key, out var existingEntity))
            {
                // Update existing entity
                var entry = dbSet.Entry(existingEntity);

                // Get all tracked properties
                var properties = entry.Properties.Select(p => p.Metadata.Name);

                foreach (var property in properties)
                {
                    if (excludeSet.Contains(property))
                        continue;

                    if (propertyInfoCache.TryGetValue(property, out var propertyInfo))
                    {
                        var oldValue = propertyInfo.GetValue(existingEntity);
                        var newValue = propertyInfo.GetValue(entity);

                        if (!Equals(oldValue, newValue))
                        {
                            entry.Property(property).CurrentValue = newValue;
                            entry.Property(property).IsModified = true;
                        }
                    }
                }
            }
            else
            {
                // Add new entity
                await dbSet.AddAsync(entity, cancellationToken);
            }
        }
    }
}
