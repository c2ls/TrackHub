using Common.Application.Attributes;
using Common.Application.Behaviors;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Mediator;
using FluentAssertions;
using Moq;

namespace Common.Application.Tests.Behaviors;

public class RateLimitingBehaviorTests
{
    private class NonRateLimitedRequest : IRequest<string> { }

    [RateLimiting(PermitLimit = 2, WindowSeconds = 60, PartitionKey = "user")]
    private class RateLimitedRequest : IRequest<string> { }

    [RateLimiting(PermitLimit = 2, WindowSeconds = 60, PartitionKey = "client")]
    private class ClientRateLimitedRequest : IRequest<string> { }

    [RateLimiting(PermitLimit = 2, WindowSeconds = 60, PartitionKey = "endpoint")]
    private class EndpointRateLimitedRequest : IRequest<string> { }

    [RateLimiting(PermitLimit = 2, WindowSeconds = 60, PartitionKey = "unknown")]
    private class UnknownPartitionRequest : IRequest<string> { }

    [Fact]
    public async Task Handle_NoRateLimitAttribute_ProceedsToNext()
    {
        var user = new Mock<IUser>();
        var behavior = new RateLimitingBehavior<NonRateLimitedRequest, string>(user.Object);
        var result = await behavior.HandleAsync(new NonRateLimitedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_NullPartitionKey_ProceedsToNext()
    {
        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns((string?)null);
        var behavior = new RateLimitingBehavior<RateLimitedRequest, string>(user.Object);
        var result = await behavior.HandleAsync(new RateLimitedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_WithinLimit_ProceedsToNext()
    {
        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        var behavior = new RateLimitingBehavior<RateLimitedRequest, string>(user.Object);

        var result = await behavior.HandleAsync(new RateLimitedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_ExceedsLimit_ThrowsTooManyRequests()
    {
        var userId = Guid.NewGuid().ToString();
        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(userId);

        // Use a unique request type to avoid cross-test interference - use client partition
        user.Setup(u => u.Client).Returns($"unique-client-{Guid.NewGuid()}");
        var behavior = new RateLimitingBehavior<ClientRateLimitedRequest, string>(user.Object);

        // First 2 should pass
        await behavior.HandleAsync(new ClientRateLimitedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await behavior.HandleAsync(new ClientRateLimitedRequest(), () => Task.FromResult("OK"), CancellationToken.None);

        // 3rd should throw
        var act = () => behavior.HandleAsync(new ClientRateLimitedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        var ex = await act.Should().ThrowAsync<TooManyRequestsException>();
        ex.Which.RetryAfterSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_EndpointPartitionKey_UsesRequestTypeName()
    {
        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        var behavior = new RateLimitingBehavior<EndpointRateLimitedRequest, string>(user.Object);

        var result = await behavior.HandleAsync(new EndpointRateLimitedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_UnknownPartitionKey_DefaultsToUserId()
    {
        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        var behavior = new RateLimitingBehavior<UnknownPartitionRequest, string>(user.Object);

        var result = await behavior.HandleAsync(new UnknownPartitionRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }
}
