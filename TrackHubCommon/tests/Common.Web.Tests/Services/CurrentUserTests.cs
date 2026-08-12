using System.Security.Claims;
using Common.Web.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Common.Web.Tests.Services;

public class CurrentUserTests
{
    [Fact]
    public void Id_ReturnsNameIdentifierClaim()
    {
        var userId = Guid.NewGuid().ToString();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }))
        };
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        var currentUser = new CurrentUser(accessor.Object);
        currentUser.Id.Should().Be(userId);
    }

    [Fact]
    public void Client_ReturnsClientIdClaim()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("client_id", "my-client")
            }))
        };
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        var currentUser = new CurrentUser(accessor.Object);
        currentUser.Client.Should().Be("my-client");
    }

    [Fact]
    public void Role_ReturnsRoleClaim()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "admin")
            }))
        };
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        var currentUser = new CurrentUser(accessor.Object);
        currentUser.Role.Should().Be("admin");
    }

    [Fact]
    public void AccountId_ReturnsAccountIdClaim()
    {
        var accountId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("account_id", accountId.ToString())
            }))
        };
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        var currentUser = new CurrentUser(accessor.Object);
        currentUser.AccountId.Should().Be(accountId);
    }

    [Fact]
    public void NullHttpContext_ReturnsNull()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var currentUser = new CurrentUser(accessor.Object);
        currentUser.Id.Should().BeNull();
        currentUser.Client.Should().BeNull();
        currentUser.Role.Should().BeNull();
    }

    [Fact]
    public void NoClaims_ReturnsNull()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        var currentUser = new CurrentUser(accessor.Object);
        currentUser.Id.Should().BeNull();
        currentUser.Client.Should().BeNull();
        currentUser.Role.Should().BeNull();
    }
}
