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

using Common.Application.Exceptions;
using Common.Application.Interfaces;

namespace TrackHub.Security.Application.Identity;

/// <summary>
/// Identity queries answer authorization questions about a subject (a user or a service
/// client). They are reachable by any authenticated principal, so every handler pins the
/// subject to the caller: user tokens may only ask about themselves and service-client
/// tokens only about their own client identity. Peer services propagate the original
/// caller's token on their authorization checks, so legitimate inter-service flows always
/// satisfy these rules; anything else is an authorization oracle and is rejected.
/// </summary>
public static class IdentityCallerGuard
{
    private const string ServiceRole = "service";

    public static bool IsServicePrincipal(IUser user)
        => user.PrincipalType == PrincipalType.ServiceClient
           || (user.PrincipalType == PrincipalType.Unknown && user.Role == ServiceRole);

    /// <summary>User callers may only target themselves; service clients may target any user.</summary>
    public static void EnsureCallerIsSubjectUserOrService(IUser user, Guid subjectUserId, string queryName)
    {
        if (user.Id is null)
        {
            throw new UnauthorizedAccessException();
        }

        if (IsServicePrincipal(user))
        {
            return;
        }

        if (Guid.TryParse(user.Id, out var callerId) && callerId == subjectUserId)
        {
            return;
        }

        throw new ForbiddenAccessException(
            "Identity",
            queryName,
            $"'{queryName}' may only target the calling principal.");
    }

    /// <summary>Only a service-client caller asking about its own client identity is allowed.</summary>
    public static void EnsureCallerIsSubjectService(IUser user, string? subjectClient, string queryName)
    {
        if (user.Id is null)
        {
            throw new UnauthorizedAccessException();
        }

        if (!IsServicePrincipal(user))
        {
            throw new ForbiddenAccessException(
                "Identity",
                queryName,
                $"'{queryName}' requires a service-client principal.");
        }

        var caller = user.Client ?? user.SubjectId;
        if (!string.Equals(caller, subjectClient, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenAccessException(
                "Identity",
                queryName,
                $"'{queryName}' may only target the calling principal.");
        }
    }
}
