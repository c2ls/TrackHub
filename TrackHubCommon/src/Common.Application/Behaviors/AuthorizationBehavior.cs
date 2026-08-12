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

using System.Reflection;
using Common.Application.Attributes;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Mediator;

namespace Common.Application.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse>(
    IUser user,
    IIdentityService identityService) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    // Handles the request by performing authorization checks.
    // If the user is not authorized, it throws an UnauthorizedAccessException or ForbiddenAccessException.
    // If the user is authorized or authorization is not required, it proceeds to the next handler in the pipeline.
    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        // Get the AuthorizeAttributes applied to the request type
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            // Check if the user is authenticated
            if (user.Id == null)
            {
                throw new UnauthorizedAccessException();
            }

            var principalType = ResolveEffectivePrincipalType();
            foreach (var attribute in authorizeAttributes)
                if (!IsPrincipalAllowed(principalType, attribute.PrincipalTypes))
                    throw new ForbiddenAccessException(
                        attribute.Resource,
                        attribute.Action,
                        $"Principal type '{principalType}' is not allowed. Allowed principal types: {attribute.PrincipalTypes}.");

            switch (principalType)
            {
                case PrincipalType.ServiceClient:
                    foreach (var attribute in authorizeAttributes)
                        if (!await identityService.IsValidServiceAsync(user.Client ?? user.SubjectId, attribute.Resource, attribute.Action, user.AccountId, user.Scopes, user.Audiences, cancellationToken))
                            throw new ForbiddenAccessException(attribute.Resource, attribute.Action);
                    break;

                case PrincipalType.Driver:
                    // Driver branch: a driver principal is authorized by principal type + claims only.
                    // Require a driver identity and an account scope; assignment-level checks (e.g. Manager
                    // ValidateDriverAssignment) are the handler's responsibility per the platform rules.
                    // Drivers never carry roles/policies, so the user branch is skipped entirely.
                    if (user.DriverId is not { } driverId || driverId == Guid.Empty
                        || user.AccountId is not { } driverAccountId || driverAccountId == Guid.Empty)
                    {
                        var attribute = authorizeAttributes.First();
                        throw new ForbiddenAccessException(
                            attribute.Resource,
                            attribute.Action,
                            "Driver principal requires driver_id and account_id claims.");
                    }
                    break;

                case PrincipalType.PublicLink:
                    // Public-link principals never invoke authorized operations. Public-link data access flows
                    // exclusively through Manager's anonymous resolution endpoint; reject fail-closed here.
                    {
                        var attribute = authorizeAttributes.First();
                        throw new ForbiddenAccessException(
                            attribute.Resource,
                            attribute.Action,
                            "Public-link principals are not permitted to invoke authorized operations.");
                    }

                default:
                    if (!Guid.TryParse(user.Id, out var userId))
                    {
                        throw new UnauthorizedAccessException();
                    }

                    // Iterate through each AuthorizeAttribute
                    foreach (var attribute in authorizeAttributes)
                    {
                        var resource = attribute.Resource;
                        var action = attribute.Action;

                        // Single combined role + policy decision, evaluated by Security in one call
                        if (!await identityService.AuthorizeUserAsync(userId, resource, action, cancellationToken))
                        {
                            throw new ForbiddenAccessException(resource, action);
                        }
                    }
                    break;
            }
        }

        // User is authorized / authorization not required, proceed to the next handler
        return await next();
    }

    private PrincipalType ResolveEffectivePrincipalType()
    {
        if (user.PrincipalType != PrincipalType.Unknown)
        {
            return user.PrincipalType;
        }

        if (!string.IsNullOrEmpty(user.Role) && user.Role == "service")
        {
            return PrincipalType.ServiceClient;
        }

        return user.Id == null ? PrincipalType.Unknown : PrincipalType.User;
    }

    private static bool IsPrincipalAllowed(PrincipalType principalType, string principalTypes)
    {
        if (string.IsNullOrWhiteSpace(principalTypes))
        {
            return true;
        }

        return principalTypes
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(value => Enum.TryParse<PrincipalType>(value, ignoreCase: true, out var allowed) && allowed == principalType);
    }

}
