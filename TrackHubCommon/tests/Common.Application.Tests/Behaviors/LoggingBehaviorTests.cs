using Common.Application.Behaviors;
using Common.Application.Interfaces;
using Common.Mediator;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Common.Application.Tests.Behaviors;

public class TestLoggingRequest : IRequest<string> { }

public class LoggingBehaviorTests
{
    private static ILogger<TestLoggingRequest> DebugEnabledLogger()
    {
        var logger = new Mock<ILogger<TestLoggingRequest>>();
        logger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        return logger.Object;
    }

    [Fact]
    public async Task Handle_WithUserId_LogsAndRetrievesUserName()
    {
        var user = new Mock<IUser>();
        var identityService = new Mock<IIdentityService>();
        var userId = Guid.NewGuid();

        user.Setup(u => u.Id).Returns(userId.ToString());
        identityService.Setup(s => s.GetUserNameAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync("TestUser");

        var behavior = new LoggingBehavior<TestLoggingRequest, string>(DebugEnabledLogger(), user.Object, identityService.Object);
        var result = await behavior.HandleAsync(new TestLoggingRequest(), () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
        identityService.Verify(s => s.GetUserNameAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithServiceClientSubject_DoesNotThrowAndSkipsUserNameLookup()
    {
        // Regression: client-credentials tokens carry a non-Guid subject (the client id).
        // new Guid("router_client") used to throw here and fail EVERY service-to-service request.
        var user = new Mock<IUser>();
        var identityService = new Mock<IIdentityService>();

        user.Setup(u => u.Id).Returns("router_client");

        var behavior = new LoggingBehavior<TestLoggingRequest, string>(DebugEnabledLogger(), user.Object, identityService.Object);
        var result = await behavior.HandleAsync(new TestLoggingRequest(), () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
        identityService.Verify(s => s.GetUserNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDebugLoggingDisabled_SkipsUserNameLookup()
    {
        // The display-name resolution costs an identity round-trip per request; it must only
        // run when Debug logging is actually enabled.
        var user = new Mock<IUser>();
        var identityService = new Mock<IIdentityService>();

        user.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());

        var behavior = new LoggingBehavior<TestLoggingRequest, string>(
            NullLogger<TestLoggingRequest>.Instance, user.Object, identityService.Object);
        var result = await behavior.HandleAsync(new TestLoggingRequest(), () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
        identityService.Verify(s => s.GetUserNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutUserId_DoesNotRetrieveUserName()
    {
        var logger = NullLogger<TestLoggingRequest>.Instance;
        var user = new Mock<IUser>();
        var identityService = new Mock<IIdentityService>();

        user.Setup(u => u.Id).Returns((string?)null);

        var behavior = new LoggingBehavior<TestLoggingRequest, string>(logger, user.Object, identityService.Object);
        var result = await behavior.HandleAsync(new TestLoggingRequest(), () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
        identityService.Verify(s => s.GetUserNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
