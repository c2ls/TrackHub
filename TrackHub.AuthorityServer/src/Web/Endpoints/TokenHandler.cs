// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

using Microsoft.AspNetCore;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using TrackHub.AuthorityServer.Domain.Interfaces;
using TrackHub.AuthorityServer.Domain.Models;

namespace TrackHub.AuthorityServer.Web.Endpoints;

// Responsible for handling token exchange requests.
public sealed class TokenHandler(
    IClientReader clientReader,
    IUserReader userReader,
    IDriverCredentialReader driverCredentialReader,
    IServiceClientPermissionReader serviceClientPermissionReader)
{
    // Optional client-credentials request parameter a service client with grants on more than one
    // account uses to pick which tenant the token is issued for.
    internal const string AccountIdParameter = "account_id";

    /// <summary>
    /// Handles the token exchange request.
    /// </summary>
    /// <param name="context">The HttpContext object representing the current HTTP request.</param>
    /// <returns>An asynchronous task that represents the token exchange operation.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IResult> Exchange(HttpContext context)
    {
        // Retrieve the OpenID Connect request from the context
        var request = context.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Check if the grant type is supported
        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the authorization code
            var claimsPrincipal = (await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal
                ?? throw new NotImplementedException("The specified grant type is not implemented.");

            // On refresh, re-validate the subject so that a deactivated/locked user or a driver whose
            // credential was revoked cannot keep exchanging refresh tokens for the token's full lifetime.
            if (request.IsRefreshTokenGrantType())
            {
                if (!await IsSubjectStillValidAsync(claimsPrincipal, context.RequestAborted))
                {
                    return Results.Forbid(
                        authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The subject associated with the refresh token is no longer valid."
                        }));
                }

                // Security stays the source of truth for roles: the token's role claim is
                // re-resolved on every refresh, so a role change propagates within one
                // access-token lifetime instead of waiting for a re-login.
                await RefreshUserRoleClaimAsync(claimsPrincipal, context.RequestAborted);
            }

            // Return the result based on the claims principal
            return Results.SignIn(claimsPrincipal, properties: null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        else if (request.IsClientCredentialsGrantType())
        {
            // Validate the client credentials
            var applicationManager = context.RequestServices.GetRequiredService<IOpenIddictApplicationManager>();
            if (string.IsNullOrEmpty(request.ClientId) || string.IsNullOrEmpty(request.ClientSecret)) 
            {
                throw new InvalidOperationException("The client credentials are missing.");
            }

            var application = await applicationManager.FindByClientIdAsync(request.ClientId, context.RequestAborted);

            if (application == null || !await applicationManager.ValidateClientSecretAsync(application, request.ClientSecret, context.RequestAborted))
            {
                return Results.Unauthorized();
            }

            //Retrieve the user linked to the client if available
            var client = await clientReader.GetClientAsync(request.ClientId, context.RequestAborted);
            var subject = client != default && client.UserId.HasValue ? client.UserId.Value.ToString() : request.ClientId;

            // Create a new claims principal
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.AddClaim(OpenIddictConstants.Claims.Subject, subject,
                OpenIddictConstants.Destinations.AccessToken);

            identity.AddClaim(new Claim(ClaimTypes.Role, "service")
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken));

            identity.AddClaim(new Claim("client_id", request.ClientId)
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken));

            identity.AddClaim(new Claim("principal_type", "ServiceClient")
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken));

            // Bind the token to the tenant the client's permission grants declare, so an
            // account-scoped partner credential cannot reach another account's data.
            var accountScope = await serviceClientPermissionReader.GetAccountScopeAsync(request.ClientId, context.RequestAborted);
            var (accountId, accountError) = ResolveServiceClientAccount(
                request.GetParameter(AccountIdParameter)?.ToString(), accountScope);

            if (accountError is not null)
            {
                return Results.Forbid(
                    authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidRequest,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = accountError
                    }));
            }

            if (accountId.HasValue)
            {
                identity.AddClaim(new Claim("account_id", accountId.Value.ToString())
                    .SetDestinations(OpenIddictConstants.Destinations.AccessToken));
            }

            var principal = new ClaimsPrincipal(identity);

            // Set the scopes granted to the client application
            principal.SetScopes(request.GetScopes());

            // Resolve and set resources (audience) from the granted scopes.
            var scopeManager = context.RequestServices.GetRequiredService<IOpenIddictScopeManager>();
            principal.SetResources(await scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

            // Return the result based on the claims principal
            return Results.SignIn(principal, properties: null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        else
        {
            throw new InvalidOperationException("The specified grant type is not supported.");
        }
    }

    // Decides the account claim of a client-credentials token from the client's effective grants.
    //   * A client holding a declared platform-wide grant stays UNSCOPED (no claim): a cross-account
    //     grant matches regardless of the token's account, and Security's matcher rejects an
    //     account-bearing token against an unbound grant — so claiming an account here would break
    //     every internal identity (router/syncworker/security/geofence/trip clients).
    //   * A client whose grants all name the same single account gets bound to it.
    //   * A client with grants on several accounts is ambiguous and must name the tenant it wants
    //     through the account_id request parameter; issuing an arbitrary one would silently give a
    //     partner the wrong tenant.
    //   * A client with no account-bound grant at all stays unscoped, as before.
    internal static (Guid? AccountId, string? Error) ResolveServiceClientAccount(
        string? requestedAccountId,
        ServiceClientAccountScopeVm scope)
    {
        if (!string.IsNullOrWhiteSpace(requestedAccountId))
        {
            if (!Guid.TryParse(requestedAccountId, out var requested))
            {
                return (null, "The account_id parameter is not a valid identifier.");
            }

            return scope.AccountIds.Contains(requested)
                ? (requested, null)
                : (null, "The client has no active permission grant for the requested account.");
        }

        if (scope.AllowsCrossAccount || scope.AccountIds.Count == 0)
        {
            return (null, null);
        }

        return scope.AccountIds.Count == 1
            ? (scope.AccountIds.Single(), null)
            : (null, "The client holds grants on several accounts; specify the account_id parameter.");
    }

    // Replaces the role claim with the user's current role (most privileged, from security.user_role)
    // so refreshed access tokens track role changes without a re-login.
    internal async Task RefreshUserRoleClaimAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var principalType = principal.FindFirst("principal_type")?.Value ?? "User";
        if (!string.Equals(principalType, "User", StringComparison.OrdinalIgnoreCase)
            || principal.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        var subject = principal.FindFirst("user_id")?.Value
            ?? principal.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (!Guid.TryParse(subject, out var userId))
        {
            return;
        }

        var role = await userReader.GetUserRoleAsync(userId, cancellationToken);
        foreach (var stale in identity.FindAll(ClaimTypes.Role).ToList())
        {
            identity.RemoveClaim(stale);
        }

        if (!string.IsNullOrEmpty(role))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role)
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken));
        }
    }

    // Re-validates the principal behind a refresh token against the current security state.
    private async Task<bool> IsSubjectStillValidAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var principalType = principal.FindFirst("principal_type")?.Value ?? "User";

        if (string.Equals(principalType, "Driver", StringComparison.OrdinalIgnoreCase))
        {
            return Guid.TryParse(principal.FindFirst("driver_id")?.Value, out var driverId)
                && await driverCredentialReader.HasActiveCredentialAsync(driverId, cancellationToken);
        }

        // Client-credentials tokens carry no refresh token, so nothing to re-validate for them.
        if (string.Equals(principalType, "ServiceClient", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var subject = principal.FindFirst("user_id")?.Value
            ?? principal.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;

        if (!Guid.TryParse(subject, out var userId))
        {
            return false;
        }

        var user = await userReader.GetUserAsync(userId, cancellationToken);
        return user != default
            && user.Active
            && (user.LockedUntil is null || user.LockedUntil <= DateTimeOffset.UtcNow);
    }
}
