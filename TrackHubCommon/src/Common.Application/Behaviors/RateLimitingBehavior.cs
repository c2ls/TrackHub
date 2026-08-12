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

using System.Collections.Concurrent;
using System.Reflection;
using Common.Application.Attributes;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Mediator;

namespace Common.Application.Behaviors;

public class RateLimitingBehavior<TRequest, TResponse>(IUser user) 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private static readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitStore = new();
    private static readonly Lock CleanupLock = new();
    private static DateTimeOffset LastCleanup = DateTimeOffset.UtcNow;

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        var rateLimitAttribute = request.GetType().GetCustomAttribute<RateLimitingAttribute>();

        if (rateLimitAttribute == null)
        {
            return await next();
        }

        var partitionKey = GetPartitionKey(rateLimitAttribute.PartitionKey);
        if (string.IsNullOrEmpty(partitionKey))
        {
            return await next();
        }

        var requestType = typeof(TRequest).Name;
        var key = $"{requestType}:{rateLimitAttribute.PartitionKey}:{partitionKey}";

        PerformCleanup();

        var info = _rateLimitStore.AddOrUpdate(
            key,
            _ => new RateLimitInfo(rateLimitAttribute.PermitLimit, rateLimitAttribute.WindowSeconds),
            (_, existing) => existing
        );

        if (!info.TryAcquire())
        {
            var retryAfter = (int)Math.Ceiling(info.GetRetryAfterSeconds());
            throw new TooManyRequestsException($"Rate limit exceeded. Retry after {retryAfter} seconds.")
            {
                RetryAfterSeconds = retryAfter
            };
        }

        return await next();
    }

    private string? GetPartitionKey(string partitionKeyType)
    {
        return partitionKeyType.ToLowerInvariant() switch
        {
            "user" => user.Id,
            "client" => user.Client,
            "endpoint" => typeof(TRequest).Name,
            _ => user.Id
        };
    }

    private static void PerformCleanup()
    {
        if ((DateTimeOffset.UtcNow - LastCleanup).TotalMinutes < 5)
        {
            return;
        }

        lock (CleanupLock)
        {
            if ((DateTimeOffset.UtcNow - LastCleanup).TotalMinutes < 5)
            {
                return;
            }

            var keysToRemove = _rateLimitStore
                .Where(kvp => kvp.Value.IsExpired())
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _rateLimitStore.TryRemove(key, out _);
            }

            LastCleanup = DateTimeOffset.UtcNow;
        }
    }

    private class RateLimitInfo(int permitLimit, int windowSeconds)
    {
        private readonly Queue<DateTimeOffset> _requestTimestamps = new();
        private readonly Lock _lock = new();

        public bool TryAcquire()
        {
            lock (_lock)
            {
                var now = DateTimeOffset.UtcNow;
                var windowStart = now.AddSeconds(-windowSeconds);

                while (_requestTimestamps.Count > 0 && _requestTimestamps.Peek() < windowStart)
                {
                    _requestTimestamps.Dequeue();
                }

                if (_requestTimestamps.Count >= permitLimit)
                {
                    return false;
                }

                _requestTimestamps.Enqueue(now);
                return true;
            }
        }

        public double GetRetryAfterSeconds()
        {
            lock (_lock)
            {
                if (_requestTimestamps.Count == 0)
                {
                    return 0;
                }

                var oldestRequest = _requestTimestamps.Peek();
                var windowEnd = oldestRequest.AddSeconds(windowSeconds);
                return (windowEnd - DateTimeOffset.UtcNow).TotalSeconds;
            }
        }

        public bool IsExpired()
        {
            lock (_lock)
            {
                if (_requestTimestamps.Count == 0)
                {
                    return true;
                }

                var now = DateTimeOffset.UtcNow;
                var windowStart = now.AddSeconds(-windowSeconds * 2);
                return _requestTimestamps.All(t => t < windowStart);
            }
        }
    }
}
