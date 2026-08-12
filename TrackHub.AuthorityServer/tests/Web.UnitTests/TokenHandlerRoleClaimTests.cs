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

using System.Security.Claims;
using Moq;
using OpenIddict.Abstractions;
using TrackHub.AuthorityServer.Domain.Interfaces;
using TrackHub.AuthorityServer.Web.Endpoints;

namespace TrackHub.AuthorityServer.Web.UnitTests;

// Refresh leg: Security stays the role source of truth — every
// refresh-token exchange re-resolves the role claim, so a role change propagates within one
// access-token lifetime instead of waiting for a re-login.
[TestFixture]
public class TokenHandlerRoleClaimTests
{
    private Mock<IUserReader> _userReader = null!;
    private TokenHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userReader = new Mock<IUserReader>();
        _handler = new TokenHandler(Mock.Of<IClientReader>(), _userReader.Object, Mock.Of<IDriverCredentialReader>());
    }

    private static ClaimsPrincipal UserPrincipal(Guid userId, string? currentRoleClaim)
    {
        var claims = new List<Claim>
        {
            new("principal_type", "User"),
            new("user_id", userId.ToString())
        };
        if (currentRoleClaim != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, currentRoleClaim));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    [Test]
    public async Task RefreshUserRoleClaimAsync_RoleChangedSinceLogin_ReplacesTheClaim()
    {
        var userId = Guid.NewGuid();
        _userReader.Setup(r => r.GetUserRoleAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync("Manager");
        var principal = UserPrincipal(userId, currentRoleClaim: "Administrator");

        await _handler.RefreshUserRoleClaimAsync(principal, CancellationToken.None);

        var roleClaims = principal.FindAll(ClaimTypes.Role).ToList();
        Assert.That(roleClaims, Has.Count.EqualTo(1));
        Assert.That(roleClaims[0].Value, Is.EqualTo("Manager"));
        Assert.That(roleClaims[0].GetDestinations(), Does.Contain(OpenIddictConstants.Destinations.AccessToken),
            "the refreshed role claim must be destined for the access token");
    }

    [Test]
    public async Task RefreshUserRoleClaimAsync_TokenWithoutRoleClaim_GainsTheCurrentRole()
    {
        var userId = Guid.NewGuid();
        _userReader.Setup(r => r.GetUserRoleAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync("Administrator");
        var principal = UserPrincipal(userId, currentRoleClaim: null);

        await _handler.RefreshUserRoleClaimAsync(principal, CancellationToken.None);

        Assert.That(principal.FindFirst(ClaimTypes.Role)?.Value, Is.EqualTo("Administrator"));
    }

    [Test]
    public async Task RefreshUserRoleClaimAsync_RoleRemovedSinceLogin_DropsTheClaim()
    {
        var userId = Guid.NewGuid();
        _userReader.Setup(r => r.GetUserRoleAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var principal = UserPrincipal(userId, currentRoleClaim: "Administrator");

        await _handler.RefreshUserRoleClaimAsync(principal, CancellationToken.None);

        Assert.That(principal.FindFirst(ClaimTypes.Role), Is.Null);
    }

    [Test]
    public async Task RefreshUserRoleClaimAsync_DriverPrincipal_IsLeftUntouched()
    {
        var claims = new List<Claim>
        {
            new("principal_type", "Driver"),
            new("driver_id", Guid.NewGuid().ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        await _handler.RefreshUserRoleClaimAsync(principal, CancellationToken.None);

        _userReader.Verify(r => r.GetUserRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task RefreshUserRoleClaimAsync_UnparseableSubject_IsLeftUntouched()
    {
        var claims = new List<Claim>
        {
            new("principal_type", "User"),
            new("user_id", "not-a-guid"),
            new(ClaimTypes.Role, "Administrator")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        await _handler.RefreshUserRoleClaimAsync(principal, CancellationToken.None);

        Assert.That(principal.FindFirst(ClaimTypes.Role)?.Value, Is.EqualTo("Administrator"));
        _userReader.Verify(r => r.GetUserRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
