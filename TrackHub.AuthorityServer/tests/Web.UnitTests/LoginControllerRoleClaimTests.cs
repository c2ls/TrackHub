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
using Common.Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TrackHub.AuthorityServer.Application.Users.Queries.GetUserRole;
using TrackHub.AuthorityServer.Application.Users.Queries.GetUsers;
using TrackHub.AuthorityServer.Domain.Models;
using TrackHub.AuthorityServer.Web.Controllers;
using TrackHub.AuthorityServer.Web.Models;

namespace TrackHub.AuthorityServer.Web.UnitTests;

// Login leg: the auth cookie carries the user's role so the authorize
// endpoint can forward it into the access token (before, user tokens had no role claim and
// IsPrivileged never fired for portal users).
[TestFixture]
public class LoginControllerRoleClaimTests
{
    private Mock<ISender> _sender = null!;
    private Mock<IAuthenticationService> _authentication = null!;
    private ClaimsPrincipal? _signedInPrincipal;
    private LoginController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _sender = new Mock<ISender>();
        _signedInPrincipal = null;
        _authentication = new Mock<IAuthenticationService>();
        _authentication
            .Setup(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string?>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties?>()))
            .Callback<HttpContext, string?, ClaimsPrincipal, AuthenticationProperties?>((_, _, principal, _) => _signedInPrincipal = principal)
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection()
            .AddSingleton(_authentication.Object)
            .BuildServiceProvider();

        // A configured portal origin so the login view can render its status-page link.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AllowedCorsOrigins"] = "https://portal.test" })
            .Build();

        _controller = new LoginController(_sender.Object, Mock.Of<IStringLocalizer<LoginController>>(), NullLogger<LoginController>.Instance, configuration)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            }
        };
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    private static UserVm User(Guid userId) => new(
        userId, "admin", "hashed", "email@mail.com", DateTimeOffset.UtcNow, true, 0, null, Guid.NewGuid());

    private async Task<ClaimsPrincipal> LoginAsync(Guid userId, string? role)
    {
        _sender.Setup(s => s.Send(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(User(userId));
        _sender.Setup(s => s.Send(It.IsAny<GetUserRoleQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(role);

        var result = await _controller.Index(new LoginViewModel { Email = "email@mail.com", Password = "12345678", ReturnUrl = "/connect/authorize?client_id=web_client" });

        Assert.That(result, Is.TypeOf<RedirectResult>());
        Assert.That(_signedInPrincipal, Is.Not.Null, "the login must sign in a cookie principal");
        return _signedInPrincipal!;
    }

    [Test]
    public async Task Index_UserWithRole_PutsTheRoleClaimInTheCookiePrincipal()
    {
        var principal = await LoginAsync(Guid.NewGuid(), "Administrator");

        Assert.That(principal.FindFirst(ClaimTypes.Role)?.Value, Is.EqualTo("Administrator"));
    }

    [Test]
    public async Task Index_UserWithoutRole_SignsInWithoutARoleClaim()
    {
        var principal = await LoginAsync(Guid.NewGuid(), null);

        Assert.That(principal.FindFirst(ClaimTypes.Role), Is.Null);
    }

    [Test]
    public async Task Index_UserLogin_KeepsTheIdentityClaims()
    {
        var userId = Guid.NewGuid();
        var principal = await LoginAsync(userId, "Manager");

        Assert.That(principal.FindFirst("user_id")?.Value, Is.EqualTo(userId.ToString()));
        Assert.That(principal.FindFirst("principal_type")?.Value, Is.EqualTo("User"));
        Assert.That(principal.FindFirst("account_id")?.Value, Is.Not.Null.And.Not.Empty);
    }
}
