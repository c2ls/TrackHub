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

namespace TrackHub.Router.Domain.Constants;

/// <summary>
/// GraphQL wire literals for Telemetry's <c>OperatorHealthCheckType</c> enum, which is the producer
/// side of <c>recordOperatorHealthCheck</c>.
/// </summary>
/// <remarks>
/// These are a CONTRACT with another service, not free text. HotChocolate coerces an enum-typed
/// variable before the resolver runs, so an unknown literal fails the whole request — and the health
/// write is best-effort (catch + log), which makes such a failure invisible to the caller.
///
/// Layer A contract tests validate the query DOCUMENT against the producer's schema and cannot see
/// values that travel in variables (rules.md, *Inter-service GraphQL client rules*). Centralising the
/// literals here lets the Layer B round-trip cases derive from <see cref="All"/> rather than being
/// hand-maintained alongside it, so an unlisted literal cannot exist.
///
/// Keep in step with <c>TrackHub.Telemetry.Domain.Enums.OperatorHealthCheckType</c>.
/// </remarks>
public static class OperatorHealthCheckTypes
{
    /// <summary>Connectivity probe — the manual "Ping" action and the worker's health loop.</summary>
    public const string Ping = "PING";

    /// <summary>Device-catalog synchronisation cycle.</summary>
    public const string DeviceSync = "DEVICE_SYNC";

    /// <summary>Position-retrieval cycle.</summary>
    public const string PositionSync = "POSITION_SYNC";

    /// <summary>Provider token/session refresh.</summary>
    public const string TokenRefresh = "TOKEN_REFRESH";

    /// <summary>Every literal this service is allowed to send. Test cases derive from this.</summary>
    public static readonly IReadOnlyList<string> All = [Ping, DeviceSync, PositionSync, TokenRefresh];

    /// <summary>True when <paramref name="value"/> is a literal the producer's enum accepts.</summary>
    public static bool IsValid(string? value)
        => value is not null && All.Contains(value, StringComparer.Ordinal);
}
