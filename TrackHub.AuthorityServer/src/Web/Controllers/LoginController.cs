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
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TrackHub.AuthorityServer.Application.Users.Queries.GetUserRole;
using TrackHub.AuthorityServer.Application.Users.Queries.GetUsers;
using System.Security.Authentication;
using Microsoft.Extensions.Localization;
using Common.Application.Exceptions;
using TrackHub.AuthorityServer.Application.Drivers.Queries.AuthenticateDriver;
using TrackHub.AuthorityServer.Web.Models;

namespace TrackHub.AuthorityServer.Web.Controllers;

public class LoginController(ISender sender, IStringLocalizer<LoginController> localizer, ILogger<LoginController> logger, IConfiguration configuration) : Controller
{
    private const string DriverMobileClientId = "driver_mobile_client";

    // GET: /Login
    // Returns the login view.
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { PlatformStatusUrl = PlatformStatusUrl() });
    }

    /// <summary>
    /// Absolute URL of the portal's public status page, so a user whose sign-in failed can check
    /// whether the platform is the cause. Derived from the already-configured portal origin
    /// (<c>AllowedCorsOrigins</c>) rather than hardcoded; returns null when it is unset or
    /// unusable, in which case the view simply omits the link.
    /// </summary>
    private string? PlatformStatusUrl()
    {
        var configured = configuration.GetSection("AllowedCorsOrigins").Get<string>();
        if (string.IsNullOrWhiteSpace(configured))
        {
            return null;
        }

        // The setting is a single origin today, but its name is plural — take the
        // first entry so a comma-separated value can never produce a malformed URL.
        var origin = configured.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();

        return Uri.TryCreate(origin, UriKind.Absolute, out var baseUri)
            ? new Uri(baseUri, "/status").ToString()
            : null;
    }

    // POST: /Login
    // Handles the login form submission.
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        ViewData["ReturnUrl"] = model.ReturnUrl;
        // Re-resolved on every POST: a failed sign-in re-renders this view, and that is
        // precisely when the user needs the status link (it is not round-tripped by the form).
        model.PlatformStatusUrl = PlatformStatusUrl();

        if (!ModelState.IsValid)
        {
            model.AuthenticationFailed = true;
            model.AuthenticationFailedMessage = localizer["CorrectErrors"];
            return View(model);
        }

        try
        {
            if (IsDriverMobileLogin(model.ReturnUrl))
            {
                var driver = await sender.Send(new AuthenticateDriverQuery(model.Email, model.Password));
                var driverClaims = new List<Claim>
                {
                    new(ClaimTypes.Sid, driver.DriverId.ToString()),
                    new("principal_type", "Driver"),
                    new("driver_id", driver.DriverId.ToString()),
                    new("account_id", driver.AccountId.ToString()),
                    new("client_id", DriverMobileClientId)
                };

                await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(driverClaims, CookieAuthenticationDefaults.AuthenticationScheme)));
                return Redirect(model.ReturnUrl);
            }

            var user = await sender.Send(new GetUsersQuery(model.Email, model.Password));

            // Create claims for the authenticated user.
            var claims = new List<Claim>
            {
                new(ClaimTypes.Sid, $"{user.UserId}"),
                new("principal_type", "User"),
                new("user_id", $"{user.UserId}"),
                new("account_id", $"{user.AccountId}")
            };

            // The role claim feeds ICurrentPrincipal.Role on every resource service (privileged
            // account-wide reads, role-addressed notifications). Users without a role row simply
            // carry no role claim.
            var role = await sender.Send(new GetUserRoleQuery(user.UserId));
            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create a claims identity using the claims and the default authentication scheme.
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Sign in the user with the created claims identity.
            await HttpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));

            // Redirect the user to the return URL.
            return Redirect(model.ReturnUrl);
        }
        catch (AuthenticationException ex)
        {
            model.AuthenticationFailedMessage = localizer[ex.Message] ?? localizer["UnknownError"];
        }
        catch (ValidationException ex)
        {
            var messages = ex.Errors.SelectMany(e => e.Value);
            model.AuthenticationFailedMessage = string.Join(" ", messages);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during login attempt for {Email}", model.Email);
            model.AuthenticationFailedMessage = localizer["UnknownError"];
        }

        model.AuthenticationFailed = true;
        return View(model);
    }

    private static bool IsDriverMobileLogin(string? returnUrl)
        => !string.IsNullOrWhiteSpace(returnUrl)
        && Uri.UnescapeDataString(returnUrl).Contains($"client_id={DriverMobileClientId}", StringComparison.OrdinalIgnoreCase);
}
