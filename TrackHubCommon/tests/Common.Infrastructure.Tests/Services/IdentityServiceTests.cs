using Common.Application.Interfaces;
using Common.Infrastructure;
using FluentAssertions;
using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Common.Infrastructure.Tests.Services;

public class IdentityServiceTests
{
    private readonly Mock<IGraphQLClient> _mockClient = new();
    private readonly Mock<IGraphQLClientFactory> _mockFactory = new();
    private readonly IdentityService _service;

    public IdentityServiceTests()
    {
        _mockFactory.Setup(f => f.CreateClient("Identity")).Returns(_mockClient.Object);
        _service = CreateService();
    }

    private IdentityService CreateService(int? cacheSeconds = null)
    {
        var settings = new Dictionary<string, string?>();
        if (cacheSeconds.HasValue)
        {
            settings["AppSettings:AuthorizationCacheSeconds"] = cacheSeconds.Value.ToString();
        }
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new IdentityService(_mockFactory.Object, new MemoryCache(new MemoryCacheOptions()), configuration);
    }

    [Fact]
    public async Task GetUserNameAsync_ReturnsUserName()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"userName":"John"}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _service.GetUserNameAsync(Guid.NewGuid(), CancellationToken.None);
        result.Should().Be("John");
    }

    [Fact]
    public async Task AuthorizeAsync_ReturnsTrue()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"authorize":true}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _service.AuthorizeAsync(Guid.NewGuid(), "Resource", "Read", CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsInRoleAsync_ReturnsFalse()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"isInRole":false}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _service.IsInRoleAsync(Guid.NewGuid(), "Resource", "Write", CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidServiceAsync_ReturnsTrue()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"isValidService":true}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _service.IsValidServiceAsync("Hub", CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeUserAsync_ReturnsCombinedDecision()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"authorizeUser":true}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _service.AuthorizeUserAsync(Guid.NewGuid(), "Resource", "Read", CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeUserAsync_CachesDecisionWithinTtl()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"authorizeUser":true}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var userId = Guid.NewGuid();
        await _service.AuthorizeUserAsync(userId, "Resource", "Read", CancellationToken.None);
        await _service.AuthorizeUserAsync(userId, "Resource", "Read", CancellationToken.None);

        _mockClient.Verify(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthorizeUserAsync_DistinctSubjectsAreNotShared()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"authorizeUser":true}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        await _service.AuthorizeUserAsync(Guid.NewGuid(), "Resource", "Read", CancellationToken.None);
        await _service.AuthorizeUserAsync(Guid.NewGuid(), "Resource", "Read", CancellationToken.None);

        _mockClient.Verify(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AuthorizeUserAsync_ZeroTtlDisablesCache()
    {
        var service = CreateService(cacheSeconds: 0);
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"authorizeUser":true}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var userId = Guid.NewGuid();
        await service.AuthorizeUserAsync(userId, "Resource", "Read", CancellationToken.None);
        await service.AuthorizeUserAsync(userId, "Resource", "Read", CancellationToken.None);

        _mockClient.Verify(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task IsValidServiceAsync_FullOverload_CachesDecisionWithinTtl()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"isValidServiceForResource":true}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result1 = await _service.IsValidServiceAsync("router_client", "Devices", "Read", null, ["service_scope"], ["trackhub_api"], CancellationToken.None);
        var result2 = await _service.IsValidServiceAsync("router_client", "Devices", "Read", null, ["service_scope"], ["trackhub_api"], CancellationToken.None);

        result1.Should().BeTrue();
        result2.Should().BeTrue();
        _mockClient.Verify(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserNameAsync_WithGraphQLError_ThrowsException()
    {
        var response = new GraphQLResponse<object>
        {
            Data = new object(),
            Errors = [new GraphQLError { Message = "Not found" }]
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var act = () => _service.GetUserNameAsync(Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<HotChocolate.GraphQLException>();
    }
}
