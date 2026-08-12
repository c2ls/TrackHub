using Common.Application.Attributes;
using Common.Application.Behaviors;
using Common.Mediator;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace Common.Application.Tests.Behaviors;

public class CachingBehaviorTests
{
    private class NonCachedRequest : IRequest<string> { }

    [Caching]
    private class CachedRequest : IRequest<string>
    {
        public bool EnableCaching { get; set; } = true;
    }

    [Caching]
    private class CachedNoPropertyRequest : IRequest<string> { }

    [Fact]
    public async Task Handle_NoCacheAttribute_ProceedsToNext()
    {
        var cache = new Mock<IDistributedCache>();
        var behavior = new CachingBehavior<NonCachedRequest, string>(cache.Object);
        var result = await behavior.HandleAsync(new NonCachedRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_CachingDisabled_ProceedsToNext()
    {
        var cache = new Mock<IDistributedCache>();
        var behavior = new CachingBehavior<CachedRequest, string>(cache.Object);
        var request = new CachedRequest { EnableCaching = false };
        var result = await behavior.HandleAsync(request, () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_CacheMiss_InvokesNextAndCaches()
    {
        var cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var behavior = new CachingBehavior<CachedRequest, string>(cache.Object);
        var result = await behavior.HandleAsync(new CachedRequest(), () => Task.FromResult("Hello"), CancellationToken.None);
        result.Should().Be("Hello");

        cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CacheHit_ReturnsCachedValue()
    {
        var cache = new Mock<IDistributedCache>();
        var cached = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize("Cached"));
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var nextCalled = false;
        var behavior = new CachingBehavior<CachedRequest, string>(cache.Object);
        var result = await behavior.HandleAsync(new CachedRequest(), () =>
        {
            nextCalled = true;
            return Task.FromResult("Fresh");
        }, CancellationToken.None);

        result.Should().Be("Cached");
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NoEnableCachingProperty_DefaultsToEnabled()
    {
        var cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var behavior = new CachingBehavior<CachedNoPropertyRequest, string>(cache.Object);
        var result = await behavior.HandleAsync(new CachedNoPropertyRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }
}
