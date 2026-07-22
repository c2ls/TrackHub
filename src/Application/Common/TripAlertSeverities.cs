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

namespace TrackHub.TripManagement.Application.Common;

/// <summary>
/// Severity literals mirroring Manager's <c>AlertSeverities</c> catalog. They travel in GraphQL
/// variables and are therefore invisible to Layer A contract validation — each one needs a Layer B
/// round-trip case (rules.md).
/// </summary>
public static class TripAlertSeverities
{
    public const string Info = nameof(Info);
    public const string Warning = nameof(Warning);
    public const string High = nameof(High);
    public const string Critical = nameof(Critical);
}
