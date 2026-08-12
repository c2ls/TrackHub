using System.Text;
using System.Text.Json;
using Common.Domain.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace Common.Domain.Tests.Extensions;

public class CachingExtensionsTests
{
    private class TestRequest
    {
        public string? Name { get; set; }
        public int Id { get; set; }
    }

    [Fact]
    public void GetCacheKey_GeneratesKeyWithProperties()
    {
        var request = new TestRequest { Name = "test", Id = 5 };
        var key = request.GetCacheKey<TestRequest, string>();
        key.Should().Contain("TestRequest");
        key.Should().Contain("Name:test");
        key.Should().Contain("Id:5");
    }

    [Fact]
    public void GetCacheKey_SameInput_SameKey()
    {
        var r1 = new TestRequest { Name = "test", Id = 5 };
        var r2 = new TestRequest { Name = "test", Id = 5 };
        r1.GetCacheKey<TestRequest, string>().Should().Be(r2.GetCacheKey<TestRequest, string>());
    }

    [Fact]
    public void GetCacheKey_DifferentInput_DifferentKey()
    {
        var r1 = new TestRequest { Name = "test1", Id = 1 };
        var r2 = new TestRequest { Name = "test2", Id = 2 };
        r1.GetCacheKey<TestRequest, string>().Should().NotBe(r2.GetCacheKey<TestRequest, string>());
    }

    [Fact]
    public async Task SetAsync_SerializesAndStoresValue()
    {
        var mockCache = new Mock<IDistributedCache>();
        byte[]? storedData = null;
        mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (_, data, _, _) => storedData = data)
            .Returns(Task.CompletedTask);

        var value = new TestRequest { Name = "cached", Id = 99 };
        await mockCache.Object.SetAsync("key", value, new DistributedCacheEntryOptions(), CancellationToken.None);

        storedData.Should().NotBeNull();
        var deserialized = JsonSerializer.Deserialize<TestRequest>(Encoding.UTF8.GetString(storedData!));
        deserialized!.Name.Should().Be("cached");
    }

    [Fact]
    public async Task GetAsync_WhenCacheHit_ReturnsDeserializedValue()
    {
        var mockCache = new Mock<IDistributedCache>();
        var value = new TestRequest { Name = "hit", Id = 1 };
        var serialized = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        mockCache.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>())).ReturnsAsync(serialized);

        var result = await mockCache.Object.GetAsync<TestRequest>("key", CancellationToken.None);
        result.Should().NotBeNull();
        result!.Name.Should().Be("hit");
    }

    [Fact]
    public async Task GetAsync_WhenCacheMiss_ReturnsDefault()
    {
        var mockCache = new Mock<IDistributedCache>();
        mockCache.Setup(c => c.GetAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        var result = await mockCache.Object.GetAsync<TestRequest>("missing", CancellationToken.None);
        result.Should().BeNull();
    }
}
