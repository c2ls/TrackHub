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

using Common.Application.Interfaces;

namespace TrackHub.Geofencing.Application.Common;

/// <summary>
/// The single visibility resolver for this module's user-facing reads, mirroring the platform
/// rule the live map applies (Telemetry's VisibleTransporterReader and TripVisibility):
/// Administrator/Manager roles and service clients read account-wide; the plain User role is
/// narrowed to the transporters in the groups they belong to via
/// <c>geofencing.vw_visible_transporter</c>.
/// </summary>
public static class GeofenceVisibility
{
    /// <summary>
    /// The caller's user id, or <c>null</c> when the principal sees the whole account.
    /// </summary>
    public static Guid? ResolveScopeUserId(IUser user, Guid userId)
        => SeesWholeAccount(user) ? null : userId;

    /// <summary>True when the principal is not group-scoped at all.</summary>
    public static bool SeesWholeAccount(IUser user)
        => user.PrincipalType == PrincipalType.ServiceClient
        || string.Equals(user.Role, Roles.Administrator, StringComparison.OrdinalIgnoreCase)
        || string.Equals(user.Role, Roles.Manager, StringComparison.OrdinalIgnoreCase);
}
