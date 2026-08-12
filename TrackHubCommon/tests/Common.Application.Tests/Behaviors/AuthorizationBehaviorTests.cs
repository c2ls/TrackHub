using Common.Application.Attributes;
using Common.Application.Behaviors;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Mediator;
using FluentAssertions;
using Moq;

namespace Common.Application.Tests.Behaviors;

public class AuthorizationBehaviorTests
{
    private readonly Mock<IUser> _userMock = new();
    private readonly Mock<IIdentityService> _identityServiceMock = new();

    [Authorize(Resource = "Users", Action = "Read")]
    private class AuthorizedRequest : IRequest<string> { }

    [Authorize(Resource = "Positions", Action = "Read", PrincipalTypes = "Driver")]
    private class DriverAuthorizedRequest : IRequest<string> { }

    [Authorize(Resource = "Positions", Action = "Read", PrincipalTypes = "User,PublicLink")]
    private class PublicLinkAllowedRequest : IRequest<string> { }

    private class NonAuthorizedRequest : IRequest<string> { }

    [Fact]
    public async Task Handle_NoAuthorizeAttribute_ProceedsToNext()
    {
        var behavior = new AuthorizationBehavior<NonAuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var result = await behavior.HandleAsync(new NonAuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_AuthorizeAttribute_NullUserId_ThrowsUnauthorized()
    {
        _userMock.Setup(u => u.Id).Returns((string?)null);
        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var act = () => behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ServiceRole_ValidService_ProceedsToNext()
    {
        var userId = Guid.NewGuid().ToString();
        _userMock.Setup(u => u.Id).Returns(userId);
        _userMock.Setup(u => u.Role).Returns("service");
        _userMock.Setup(u => u.Client).Returns("TestClient");
        _identityServiceMock.Setup(s => s.IsValidServiceAsync("TestClient", "Users", "Read", It.IsAny<Guid?>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var result = await behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_ServiceRole_InvalidServicePermission_ThrowsForbidden()
    {
        var userId = Guid.NewGuid().ToString();
        _userMock.Setup(u => u.Id).Returns(userId);
        _userMock.Setup(u => u.Role).Returns("service");
        _userMock.Setup(u => u.Client).Returns("BadClient");
        _identityServiceMock.Setup(s => s.IsValidServiceAsync("BadClient", "Users", "Read", It.IsAny<Guid?>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var act = () => behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        var exception = await act.Should().ThrowAsync<ForbiddenAccessException>();
        exception.Which.Resource.Should().Be("Users");
        exception.Which.Action.Should().Be("Read");
    }

    [Fact]
    public async Task Handle_User_Authorized_ProceedsToNext()
    {
        var userId = Guid.NewGuid();
        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userMock.Setup(u => u.Role).Returns("admin");
        _identityServiceMock.Setup(s => s.AuthorizeUserAsync(userId, "Users", "Read", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var result = await behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_User_Authorized_MakesExactlyOneIdentityCall()
    {
        var userId = Guid.NewGuid();
        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userMock.Setup(u => u.Role).Returns("admin");
        _identityServiceMock.Setup(s => s.AuthorizeUserAsync(userId, "Users", "Read", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        await behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);

        _identityServiceMock.Verify(s => s.AuthorizeUserAsync(userId, "Users", "Read", It.IsAny<CancellationToken>()), Times.Once);
        _identityServiceMock.Verify(s => s.IsInRoleAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _identityServiceMock.Verify(s => s.AuthorizeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_User_NotAuthorized_ThrowsForbidden()
    {
        var userId = Guid.NewGuid();
        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userMock.Setup(u => u.Role).Returns("user");
        _identityServiceMock.Setup(s => s.AuthorizeUserAsync(userId, "Users", "Read", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var act = () => behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        var exception = await act.Should().ThrowAsync<ForbiddenAccessException>();
        exception.Which.Resource.Should().Be("Users");
        exception.Which.Action.Should().Be("Read");
    }

    [Fact]
    public async Task Handle_Driver_DefaultPrincipalTypes_ThrowsForbidden()
    {
        // A Driver principal is rejected by a request that does not allow PrincipalTypes = Driver.
        _userMock.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _userMock.Setup(u => u.PrincipalType).Returns(PrincipalType.Driver);
        _userMock.Setup(u => u.DriverId).Returns(Guid.NewGuid());
        _userMock.Setup(u => u.AccountId).Returns(Guid.NewGuid());

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var act = () => behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_Driver_Allowed_WithClaims_SkipsRolePolicyChecks()
    {
        // AC 3: a valid Driver token is accepted without consulting user role/policy tables.
        _userMock.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _userMock.Setup(u => u.PrincipalType).Returns(PrincipalType.Driver);
        _userMock.Setup(u => u.DriverId).Returns(Guid.NewGuid());
        _userMock.Setup(u => u.AccountId).Returns(Guid.NewGuid());

        var behavior = new AuthorizationBehavior<DriverAuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var result = await behavior.HandleAsync(new DriverAuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
        _identityServiceMock.Verify(s => s.IsInRoleAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _identityServiceMock.Verify(s => s.AuthorizeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _identityServiceMock.Verify(s => s.AuthorizeUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Driver_Allowed_MissingDriverId_ThrowsForbidden()
    {
        // AC 2: a Driver-allowed request rejects a token lacking driver_id/account_id.
        _userMock.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _userMock.Setup(u => u.PrincipalType).Returns(PrincipalType.Driver);
        _userMock.Setup(u => u.DriverId).Returns((Guid?)null);
        _userMock.Setup(u => u.AccountId).Returns((Guid?)null);

        var behavior = new AuthorizationBehavior<DriverAuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var act = () => behavior.HandleAsync(new DriverAuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_Driver_Allowed_MissingAccountIdOnly_ThrowsForbidden()
    {
        // AC 2: driver_id present but account_id missing must still be rejected (guards the `||` clause).
        _userMock.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _userMock.Setup(u => u.PrincipalType).Returns(PrincipalType.Driver);
        _userMock.Setup(u => u.DriverId).Returns(Guid.NewGuid());
        _userMock.Setup(u => u.AccountId).Returns((Guid?)null);

        var behavior = new AuthorizationBehavior<DriverAuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var act = () => behavior.HandleAsync(new DriverAuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_Driver_Allowed_EmptyGuidClaims_ThrowsForbidden()
    {
        // AC 2: Guid.Empty claims are treated as absent (guards the `== Guid.Empty` checks).
        _userMock.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _userMock.Setup(u => u.PrincipalType).Returns(PrincipalType.Driver);
        _userMock.Setup(u => u.DriverId).Returns(Guid.Empty);
        _userMock.Setup(u => u.AccountId).Returns(Guid.Empty);

        var behavior = new AuthorizationBehavior<DriverAuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var act = () => behavior.HandleAsync(new DriverAuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_PublicLink_AlwaysThrowsForbidden()
    {
        // AC 5: a PublicLink principal is rejected even when the request allow-lists PublicLink.
        _userMock.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _userMock.Setup(u => u.PrincipalType).Returns(PrincipalType.PublicLink);
        _userMock.Setup(u => u.PublicLinkGrantId).Returns(Guid.NewGuid());

        var behavior = new AuthorizationBehavior<PublicLinkAllowedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var act = () => behavior.HandleAsync(new PublicLinkAllowedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_EmptyRole_ProceedsWithRoleCheck()
    {
        var userId = Guid.NewGuid();
        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userMock.Setup(u => u.Role).Returns(string.Empty);
        _identityServiceMock.Setup(s => s.AuthorizeUserAsync(userId, "Users", "Read", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(_userMock.Object, _identityServiceMock.Object);
        var result = await behavior.HandleAsync(new AuthorizedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }
}
