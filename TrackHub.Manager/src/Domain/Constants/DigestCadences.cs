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

namespace TrackHub.Manager.Domain.Constants;

/// <summary>Digest cadence values in ThrottlingJson.</summary>
public static class DigestCadences
{
    public const string None = nameof(None);
    public const string Hourly = nameof(Hourly);
    public const string Daily = nameof(Daily);

    public static readonly IReadOnlyCollection<string> All = [None, Hourly, Daily];

    public static bool IsValid(string? value) => value != null && All.Contains(value);
}
