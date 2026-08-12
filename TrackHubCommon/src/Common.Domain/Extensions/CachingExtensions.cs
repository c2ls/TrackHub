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

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Common.Domain.Extensions;

/// <summary>
/// Provides extension methods for caching operations.
/// </summary>
public static class CachingExtensions
{
    /// <summary>
    /// Generates a cache key based on the request object.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request object.</typeparam>
    /// <typeparam name="TResponse">The type of the response object.</typeparam>
    /// <param name="request">The request object.</param>
    /// <returns>The cache key.</returns>
    public static string GetCacheKey<TRequest, TResponse>(this TRequest request)
    {
        var r = new { request };
        var props = r.request?.GetType().GetProperties().Select(pi => $"{pi.Name}:{pi.GetValue(r.request, null)}");
        return $"{typeof(TRequest).FullName}{{{string.Join(",", props ?? [])}}}";
    }

    /// <summary>
    /// Sets a value in the distributed cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value to be stored in the cache.</typeparam>
    /// <param name="distributedCache">The distributed cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to be stored in the cache.</param>
    /// <param name="options">The cache entry options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SetAsync<T>(this IDistributedCache distributedCache, string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken)
    {
        var serializedValue = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        await distributedCache.SetAsync(key, serializedValue, options, cancellationToken);
    }

    /// <summary>
    /// Retrieves a value from the distributed cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the value to be retrieved from the cache.</typeparam>
    /// <param name="distributedCache">The distributed cache instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The retrieved value from the cache.</returns>
    public static async Task<T?> GetAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken cancellationToken)
    {
        var serializedValue = await distributedCache.GetAsync(key, cancellationToken);
        return serializedValue == null ? default : JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(serializedValue));
    }
}
