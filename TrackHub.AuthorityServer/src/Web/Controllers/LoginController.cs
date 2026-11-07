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

using Microsoft.AspNetCore.RateLimiting;
using TrackHub.AuthorityServer.Web.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Authorization;
using TrackHub.AuthorityServer.Application.Users.Queries.GetUserRole;
using TrackHub.AuthorityServer.Application.Users.Queries.GetUsers;
using System.Security.Authentication;
using Microsoft.Extensions.Localization;
using Common.Application.Exceptions;
using TrackHub.AuthorityServer.Application.Staff.Queries.AuthenticateStaff;
using TrackHub.AuthorityServer.Web.Models;

namespace TrackHub.AuthorityServer.Web.Controllers;

public class LoginController(ISender sender, IStringLocalizer<LoginController> localizer, ILogger<LoginController> logger, IConfiguration configuration) : Controller
{
    private const string StaffMobileClientId = "staff_mobile_client";

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
        var origin = configuration.GetAllowedCorsOrigins().FirstOrDefault();

        return Uri.TryCreate(origin, UriKind.Absolute, out var baseUri)
            ? new Uri(baseUri, "/status").ToString()
            : null;
    }

    // POST: /Login
    // Handles the login form submission.
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(CredentialRateLimit.Policy)]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        model.ReturnUrl = LocalReturnUrl(model.ReturnUrl);

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
            if (IsStaffMobileLogin(model.ReturnUrl))
            {
                var staff = await sender.Send(new AuthenticateStaffQuery(model.Email, model.Password));
                var staffClaims = new List<Claim>
                {
                    new(ClaimTypes.Sid, staff.StaffId.ToString()),
                    new("principal_type", "Staff"),
                    new("staff_id", staff.StaffId.ToString()),
                    new("staff_credential_id", staff.StaffCredentialId.ToString()),
                    new("account_id", staff.AccountId.ToString()),
                    new("client_id", StaffMobileClientId),
                    new(SubjectValidity.SecurityStampClaim, staff.SecurityStamp.ToString())
                };

                await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(staffClaims, CookieAuthenticationDefaults.AuthenticationScheme)));
                return Redirect(model.ReturnUrl);
            }

            var user = await sender.Send(new GetUsersQuery(model.Email, model.Password));

            // Create claims for the authenticated user.
            var claims = new List<Claim>
            {
                new(ClaimTypes.Sid, $"{user.UserId}"),
                new("principal_type", "User"),
                new("user_id", $"{user.UserId}"),
                new("account_id", $"{user.AccountId}"),
                new(SubjectValidity.SecurityStampClaim, $"{user.SecurityStamp}")
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

    // A substring match selected the staff branch for any url merely containing that text,
    // including inside a state or redirect_uri parameter.
    private static bool IsStaffMobileLogin(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return false;
        }

        var queryStart = returnUrl.IndexOf('?', StringComparison.Ordinal);
        if (queryStart < 0)
        {
            return false;
        }

        var query = QueryHelpers.ParseQuery(returnUrl[queryStart..]);
        return query.TryGetValue("client_id", out var clientId)
            && string.Equals(clientId.ToString(), StaffMobileClientId, StringComparison.Ordinal);
    }

    // Same rule as IUrlHelper.IsLocalUrl, without depending on MVC's url helper being wired up.
    // An absolute or protocol-relative destination is replaced rather than rejected, so a stale
    // bookmark still completes the sign-in.
    private static string LocalReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
        {
            return "/";
        }

        if (returnUrl[0] == '/')
        {
            return returnUrl.Length == 1 || (returnUrl[1] != '/' && returnUrl[1] != '\\') ? returnUrl : "/";
        }

        if (returnUrl.Length > 1 && returnUrl[0] == '~' && returnUrl[1] == '/')
        {
            return returnUrl;
        }

        return "/";
    }
}
