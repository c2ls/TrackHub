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

/// <summary>
/// Allowed <c>AccountSupportGrant.AccessLevel</c> values. A <see cref="ReadOnly"/> grant permits
/// cross-account reads only; cross-account mutations require <see cref="Full"/>.
/// </summary>
public static class SupportAccessLevels
{
    public const string ReadOnly = "ReadOnly";
    public const string Full = "Full";

    public static bool IsValid(string? accessLevel)
        => accessLevel == ReadOnly || accessLevel == Full;
}
