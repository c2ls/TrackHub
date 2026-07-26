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
using Common.Application.Exceptions;
using Common.Domain.Extensions;
using GraphQL;
using GraphQL.Client.Abstractions;
using HotChocolate;

namespace Common.Infrastructure;

// This abstract class provides a base implementation for making GraphQL queries and mutations.
// It takes an IGraphQLClient instance as a dependency to send the GraphQL requests.
public abstract class GraphQLService(IGraphQLClient graphQLClient)
{

    /// <summary>
    /// This method sends a GraphQL query request and returns the deserialized response.
    /// It throws an exception if there is an error in the GraphQL query execution or if the data string is null or empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request">GraphQLRequest object</param>
    /// <param name="token">CancellationToken</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="GraphQLException"></exception>
    public async Task<T> QueryAsync<T>(GraphQLRequest request, CancellationToken token)
    {
        var response = await graphQLClient.SendQueryAsync<object>(request, token)
            ?? throw new Exception("GraphQL query execution error.");

        if (response.Errors != null && response.Errors.Length > 0)
        {
            throw new GraphQLException(response.Errors.ConvertToIError());
        }

        var dataString = response.Data.ToString();
        return string.IsNullOrEmpty(dataString)
            ? throw new Exception("Data string is null or empty.")
            : ExtractFirstPropertyValue<T>(dataString);
    }

    /// <summary>
    /// This method sends a GraphQL mutation request and returns the deserialized response.
    /// It throws an exception if there is an error in the GraphQL mutation execution or if the data string is null or empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request">GraphQLRequest object</param>
    /// <param name="token">CancellationToken</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="GraphQLException"></exception>
    public async Task<T> MutationAsync<T>(GraphQLRequest request, CancellationToken token)
    {
        var response = await graphQLClient.SendMutationAsync<object>(request, token)
            ?? throw new Exception("GraphQL mutation execution error.");

        if (response.Errors != null && response.Errors.Length > 0)
        {
            throw new GraphQLException(response.Errors.ConvertToIError());
        }

        var dataString = response.Data.ToString();
        return string.IsNullOrEmpty(dataString)
            ? throw new Exception("Data string is null or empty.")
            : ExtractFirstPropertyValue<T>(dataString);
    }

    /// <summary>
    /// This private method extracts the first property value from a JSON string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json">string parameter representing the JSON string</param>
    /// <returns>It returns the deserialized value of the first property</returns>
    /// <exception cref="Exception">It throws an exception if the response is null or empty</exception>
    private static T ExtractFirstPropertyValue<T>(string json)
    {
        var dataObject = JsonDocument.Parse(json);
        var property = dataObject.RootElement.EnumerateObject().FirstOrDefault();
        if (property.Value.ValueKind != JsonValueKind.Null)
        {
            var propertyJson = property.Value.GetRawText();
            var propertyValue = propertyJson.Deserialize<T>();
            if (propertyValue != null)
                return propertyValue;
        }

        // A Nullable<T> return type means the caller declared null a VALID answer for this field —
        // e.g. `activeGeocodingProvider` returning null when no provider is configured. Throwing
        // here made every such null-handling branch unreachable (ReverseGeocodingService's
        // `provider is null` check could never fire) and masked the real condition behind a bare
        // Exception that no error filter maps, surfacing as "Unexpected Execution Error".
        //
        // Deliberately Nullable<T> ONLY, not `default(T) is null`: nullable reference annotations
        // are erased at runtime, so `string` and `string?` are indistinguishable here and widening
        // to all reference types would turn a genuine "field came back empty" bug into a silent
        // null. Every cross-service Vm in this platform is a record struct, so the nullable ones
        // are Nullable<T> and detectable.
        if (Nullable.GetUnderlyingType(typeof(T)) is not null)
            return default!;

        throw new Exception("Response is null or empty.");
    }
}
