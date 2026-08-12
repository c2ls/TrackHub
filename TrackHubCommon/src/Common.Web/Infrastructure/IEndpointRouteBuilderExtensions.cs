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

namespace Common.Web.Infrastructure;

// Contains extension methods for the IEndpointRouteBuilder interface.
public static class IEndpointRouteBuilderExtensions
{

    // Maps a GET request to the specified handler method.
    // - builder: The IEndpointRouteBuilder instance.
    // - handler: The handler method to be executed for the GET request.
    // - pattern: The URL pattern to match for the GET request. (optional)
    // Returns:
    // The updated IEndpointRouteBuilder instance.
    public static IEndpointRouteBuilder MapGet(this IEndpointRouteBuilder builder, Delegate handler, string pattern = "")
    {
        Guard.Against.AnonymousMethod(handler);

        builder.MapGet(pattern, handler)
            .WithName(handler.Method.Name);

        return builder;
    }

    // Maps a POST request to the specified handler method.
    // Parameters:
    // - builder: The IEndpointRouteBuilder instance.
    // - handler: The handler method to be executed for the POST request.
    // - pattern: The URL pattern to match for the POST request. (optional)
    // Returns:
    // The updated IEndpointRouteBuilder instance.
    public static IEndpointRouteBuilder MapPost(this IEndpointRouteBuilder builder, Delegate handler, string pattern = "")
    {
        Guard.Against.AnonymousMethod(handler);

        builder.MapPost(pattern, handler)
            .WithName(handler.Method.Name);

        return builder;
    }

    // Maps a PUT request to the specified handler method.
    // Parameters:
    // - builder: The IEndpointRouteBuilder instance.
    // - handler: The handler method to be executed for the PUT request.
    // - pattern: The URL pattern to match for the PUT request.
    // Returns:
    // The updated IEndpointRouteBuilder instance.
    public static IEndpointRouteBuilder MapPut(this IEndpointRouteBuilder builder, Delegate handler, string pattern)
    {
        Guard.Against.AnonymousMethod(handler);

        builder.MapPut(pattern, handler)
            .WithName(handler.Method.Name);

        return builder;
    }

    // Maps a DELETE request to the specified handler method.
    // Parameters:
    // - builder: The IEndpointRouteBuilder instance.
    // - handler: The handler method to be executed for the DELETE request.
    // - pattern: The URL pattern to match for the DELETE request.
    // Returns:
    // The updated IEndpointRouteBuilder instance.
    public static IEndpointRouteBuilder MapDelete(this IEndpointRouteBuilder builder, Delegate handler, string pattern)
    {
        Guard.Against.AnonymousMethod(handler);

        builder.MapDelete(pattern, handler)
            .WithName(handler.Method.Name);

        return builder;
    }
}
