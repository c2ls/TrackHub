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

namespace Common.Domain.Constants;

/// <summary>
/// Well-known group metadata shared across services.
/// </summary>
public static class GroupMetadata
{
    /// <summary>
    /// Name of the account's default group. Auto-provisioned transporters created by the
    /// device sync are placed here so plain (group-scoped) users can see them on the live map.
    /// Resolved by name and created on first use.
    /// </summary>
    public const string DefaultGroupName = "General";

    /// <summary>
    /// Description applied to the default group when the sync creates it on first use.
    /// </summary>
    public const string DefaultGroupDescription = "Auto-provisioned units";
}
