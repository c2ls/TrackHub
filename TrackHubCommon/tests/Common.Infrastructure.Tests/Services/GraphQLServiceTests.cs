using Common.Infrastructure;
using FluentAssertions;
using GraphQL;
using GraphQL.Client.Abstractions;
using HotChocolate;
using Moq;

namespace Common.Infrastructure.Tests.Services;

public class TestGraphQLService : GraphQLService
{
    public TestGraphQLService(IGraphQLClient client) : base(client) { }

    public Task<T> TestQuery<T>(GraphQLRequest request, CancellationToken token) =>
        QueryAsync<T>(request, token);

    public Task<T> TestMutation<T>(GraphQLRequest request, CancellationToken token) =>
        MutationAsync<T>(request, token);
}

public class GraphQLServiceTests
{
    private readonly Mock<IGraphQLClient> _mockClient = new();

    [Fact]
    public async Task QueryAsync_SuccessfulResponse_ReturnsDeserializedData()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"userName":"John"}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new TestGraphQLService(_mockClient.Object);
        var result = await service.TestQuery<string>(new GraphQLRequest { Query = "query{}" }, CancellationToken.None);
        result.Should().Be("John");
    }

    [Fact]
    public async Task QueryAsync_NullResponse_ThrowsException()
    {
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GraphQLResponse<object>)null!);

        var service = new TestGraphQLService(_mockClient.Object);
        var act = () => service.TestQuery<string>(new GraphQLRequest { Query = "query{}" }, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*query execution error*");
    }

    [Fact]
    public async Task QueryAsync_WithErrors_ThrowsGraphQLException()
    {
        var response = new GraphQLResponse<object>
        {
            Data = new object(),
            Errors = [new GraphQLError { Message = "Some error" }]
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new TestGraphQLService(_mockClient.Object);
        var act = () => service.TestQuery<string>(new GraphQLRequest { Query = "query{}" }, CancellationToken.None);
        await act.Should().ThrowAsync<GraphQLException>();
    }

    [Fact]
    public async Task QueryAsync_EmptyData_ThrowsException()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"result":null}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new TestGraphQLService(_mockClient.Object);
        var act = () => service.TestQuery<string>(new GraphQLRequest { Query = "query{}" }, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("Response is null or empty*");
    }

    // A Nullable<T> return type declares null a VALID answer for the field (e.g.
    // `activeGeocodingProvider` when no provider is configured). Before this, the bare throw above
    // fired for those too, which made ReverseGeocodingService's `provider is null` branch
    // unreachable and surfaced as an unmapped "Unexpected Execution Error" instead of
    // GEOCODER_UNAVAILABLE.
    [Fact]
    public async Task QueryAsync_NullField_WithNullableValueType_ReturnsNull()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"result":null}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new TestGraphQLService(_mockClient.Object);
        var result = await service.TestQuery<int?>(new GraphQLRequest { Query = "query{}" }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task MutationAsync_SuccessfulResponse_ReturnsDeserializedData()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"createUser":true}""")!
        };
        _mockClient.Setup(c => c.SendMutationAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new TestGraphQLService(_mockClient.Object);
        var result = await service.TestMutation<bool>(new GraphQLRequest { Query = "mutation{}" }, CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task MutationAsync_NullResponse_ThrowsException()
    {
        _mockClient.Setup(c => c.SendMutationAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GraphQLResponse<object>)null!);

        var service = new TestGraphQLService(_mockClient.Object);
        var act = () => service.TestMutation<string>(new GraphQLRequest { Query = "mutation{}" }, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*mutation execution error*");
    }

    [Fact]
    public async Task MutationAsync_WithErrors_ThrowsGraphQLException()
    {
        var response = new GraphQLResponse<object>
        {
            Data = new object(),
            Errors = [new GraphQLError { Message = "Mutation error" }]
        };
        _mockClient.Setup(c => c.SendMutationAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new TestGraphQLService(_mockClient.Object);
        var act = () => service.TestMutation<string>(new GraphQLRequest { Query = "mutation{}" }, CancellationToken.None);
        await act.Should().ThrowAsync<GraphQLException>();
    }

    [Fact]
    public async Task QueryAsync_ComplexObject_DeserializesCorrectly()
    {
        var response = new GraphQLResponse<object>
        {
            Data = System.Text.Json.JsonSerializer.Deserialize<object>("""{"user":{"Name":"John","Age":30}}""")!
        };
        _mockClient.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new TestGraphQLService(_mockClient.Object);
        var result = await service.TestQuery<TestUser>(new GraphQLRequest { Query = "query{}" }, CancellationToken.None);
        result.Should().NotBeNull();
        result.Name.Should().Be("John");
        result.Age.Should().Be(30);
    }
}

public class TestUser
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
}
