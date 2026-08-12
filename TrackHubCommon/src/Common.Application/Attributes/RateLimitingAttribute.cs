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

namespace Common.Application.Attributes;

/// <summary>
/// Specifies the class this attribute is applied to requires rate limiting.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public sealed class RateLimitingAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitingAttribute"/> class.
    /// </summary>
    public RateLimitingAttribute() { }

    /// <summary>
    /// Gets or sets the maximum number of requests allowed within the time window.
    /// Default is 100 requests.
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time window in seconds for rate limiting.
    /// Default is 60 seconds (1 minute).
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the segment size for sliding window algorithm.
    /// Default is 1 segment (fixed window).
    /// </summary>
    public int SegmentsPerWindow { get; set; } = 1;

    /// <summary>
    /// Gets or sets the queued request limit.
    /// Default is 0 (no queueing).
    /// </summary>
    public int QueueLimit { get; set; } = 0;

    /// <summary>
    /// Gets or sets the partition key resolver.
    /// Standard values: "user", "client", "endpoint"
    /// Default is "user" (rate limit per authenticated user).
    /// </summary>
    public string PartitionKey { get; set; } = "user";
}
