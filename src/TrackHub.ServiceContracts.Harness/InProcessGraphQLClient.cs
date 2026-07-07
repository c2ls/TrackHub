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

using System.Text.Json;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Serializer.SystemTextJson;
using HotChocolate;
using HotChocolate.Execution;

namespace TrackHub.ServiceContracts.Harness;

/// <summary>
/// An <see cref="IGraphQLClient"/> that forwards requests to a producer's real in-process
/// <see cref="IRequestExecutor"/> instead of HTTP. Variables are serialized with the same
/// SystemTextJson options the production <c>GraphQLHttpClient</c> uses, and the executor's
/// JSON result is deserialized back through those options, so the consumer-side
/// <c>GraphQLService</c> sees exactly the envelope it sees in production.
/// </summary>
public sealed class InProcessGraphQLClient(IRequestExecutor executor) : IGraphQLClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new SystemTextJsonSerializer().Options;

    public Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync<TResponse>(request, cancellationToken);

    public Task<GraphQLResponse<TResponse>> SendMutationAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync<TResponse>(request, cancellationToken);

    public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request)
        => throw new NotSupportedException("Subscriptions are not part of the service-to-service contract.");

    public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request, Action<Exception> exceptionHandler)
        => throw new NotSupportedException("Subscriptions are not part of the service-to-service contract.");

    public void Dispose() { }

    private async Task<GraphQLResponse<TResponse>> ExecuteAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken)
    {
        var builder = OperationRequestBuilder.New().SetDocument(request.Query!);

        var variables = ToVariableDictionary(request.Variables);
        if (variables is not null)
        {
            builder.SetVariableValues(variables);
        }

        var result = await executor.ExecuteAsync(builder.Build(), cancellationToken);
        var json = result.ToJson();
        return JsonSerializer.Deserialize<GraphQLResponse<TResponse>>(json, SerializerOptions)!;
    }

    // Production serializes the variables object into the request payload; mirror that exact
    // serialization, then hand HotChocolate plain CLR values its input coercion understands.
    private static Dictionary<string, object?>? ToVariableDictionary(object? variables)
    {
        if (variables is null)
        {
            return null;
        }

        var element = JsonSerializer.SerializeToElement(variables, SerializerOptions);
        return element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertValue(p.Value));
    }

    private static object? ConvertValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertValue(p.Value)),
        JsonValueKind.Array => element.EnumerateArray().Select(ConvertValue).ToList(),
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.TryGetInt64(out var l) ? l : element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null,
    };
}
