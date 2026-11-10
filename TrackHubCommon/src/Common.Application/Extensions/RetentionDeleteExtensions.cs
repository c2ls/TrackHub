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
using Microsoft.EntityFrameworkCore;

namespace Common.Application.Extensions;

public static class RetentionDeleteExtensions
{
    public const int DefaultChunkSize = 10_000;

    /// <summary>Deletes everything <paramref name="source"/> matches, one transaction per chunk.</summary>
    public static async Task<int> ExecuteDeleteInChunksAsync<TEntity, TKey>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, TKey>> keySelector,
        CancellationToken cancellationToken,
        int chunkSize = DefaultChunkSize)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1);

        var total = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Ordering by a constant (ORDER BY (SELECT 1)) satisfies EF's row-limit warning while the
            // planner drops it: a real key ordering would pull it onto the key index instead of the one
            // serving the retention predicate. Every probed row is deleted before the next probe runs.
            var keys = await source
                .Select(keySelector)
                .OrderBy(_ => 1)
                .Take(chunkSize)
                .ToListAsync(cancellationToken);

            if (keys.Count == 0)
            {
                return total;
            }

            total += await source
                .Where(BatchPredicate(keySelector, keys))
                .ExecuteDeleteAsync(cancellationToken);

            if (keys.Count < chunkSize)
            {
                return total;
            }
        }
    }

    private static Expression<Func<TEntity, bool>> BatchPredicate<TEntity, TKey>(
        Expression<Func<TEntity, TKey>> keySelector, List<TKey> keys)
        => Expression.Lambda<Func<TEntity, bool>>(
            Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Contains),
                [typeof(TKey)],
                Expression.Constant(keys),
                keySelector.Body),
            keySelector.Parameters[0]);
}
