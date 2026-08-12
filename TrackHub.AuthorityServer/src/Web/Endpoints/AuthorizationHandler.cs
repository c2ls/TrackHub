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

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using Microsoft.AspNetCore;

namespace TrackHub.AuthorityServer.Web.Endpoints;

// This class handles the authorization process.
public sealed class AuthorizationHandler
{
    private const string DriverMobileClientId = "driver_mobile_client";

    // This method is responsible for authorizing the request.
    public async Task Authorize(HttpContext context)
    {
        // Retrieve the OpenID Connect request from the context.
        var request = context.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the user principal stored in the authentication cookie.
        var result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // If the user principal can't be extracted, challenge the authentication scheme.
        if (!result.Succeeded)
        {
            await ChallengeWithCurrentRequestAsync(context);
            return;
        }

        var cookiePrincipal = result.Principal ?? throw new InvalidOperationException("The authentication cookie principal cannot be retrieved.");
        var principalType = cookiePrincipal.FindFirst("principal_type")?.Value ?? "User";

        if (string.Equals(request.ClientId, DriverMobileClientId, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(principalType, "Driver", StringComparison.OrdinalIgnoreCase))
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await ChallengeWithCurrentRequestAsync(context);
            return;
        }

        if (string.Equals(principalType, "Driver", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(request.ClientId, DriverMobileClientId, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var subject = cookiePrincipal.Claims.Single(x => x.Type == ClaimTypes.Sid).Value;
        var claims = new List<Claim>
        {
            AccessTokenClaim(OpenIddictConstants.Claims.Subject, subject),
            AccessTokenClaim("principal_type", principalType)
        };

        if (string.Equals(principalType, "Driver", StringComparison.OrdinalIgnoreCase))
        {
            AddRequiredClaim(cookiePrincipal, claims, "driver_id");
            AddRequiredClaim(cookiePrincipal, claims, "account_id");
            claims.Add(AccessTokenClaim("client_id", DriverMobileClientId));
        }
        else
        {
            if (!TryAddClaim(cookiePrincipal, claims, "account_id"))
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await ChallengeWithCurrentRequestAsync(context);
                return;
            }

            claims.Add(AccessTokenClaim("user_id", subject));

            // Forward the user's role into the access token (resource services derive
            // ICurrentPrincipal.Role from it). Older cookies may predate the claim — the user
            // picks it up on their next login.
            TryAddClaim(cookiePrincipal, claims, ClaimTypes.Role);
        }

        // Create a claims identity and principal.
        var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Set the scopes for the claims principal.
        claimsPrincipal.SetScopes(request.GetScopes());

        // Resolve and set resources (audience) from the granted scopes.
        var scopeManager = context.RequestServices.GetRequiredService<IOpenIddictScopeManager>();
        claimsPrincipal.SetResources(await scopeManager.ListResourcesAsync(claimsPrincipal.GetScopes()).ToListAsync());

        // Sign in the claims principal.
        if (claimsPrincipal != null)
        {
            await context.SignInAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, claimsPrincipal);
        }
    }

    private static void AddRequiredClaim(ClaimsPrincipal principal, List<Claim> claims, string type)
    {
        var value = principal.FindFirst(type)?.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"The required '{type}' claim is missing.");
        }

        claims.Add(AccessTokenClaim(type, value));
    }

    private static bool TryAddClaim(ClaimsPrincipal principal, List<Claim> claims, string type)
    {
        var value = principal.FindFirst(type)?.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        claims.Add(AccessTokenClaim(type, value));
        return true;
    }

    private static Task ChallengeWithCurrentRequestAsync(HttpContext context)
    {
        return context.ChallengeAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new AuthenticationProperties
            {
                RedirectUri = context.Request.PathBase + context.Request.Path + QueryString.Create(
                    context.Request.HasFormContentType ? [.. context.Request.Form] : context.Request.Query.ToList())
            });
    }

    private static Claim AccessTokenClaim(string type, string value)
        => new Claim(type, value).SetDestinations(OpenIddictConstants.Destinations.AccessToken);
}
