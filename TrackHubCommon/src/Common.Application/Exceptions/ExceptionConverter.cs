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

using FluentValidation.Results;
using GraphQL;
using HotChocolate;

namespace Common.Application.Exceptions;

// This class provides extension methods to convert different types of errors to IError objects
public static class ExceptionConverter
{
    // Converts a collection of GraphQLError objects to an array of IError objects
    public static IError[] ConvertToIError(this IEnumerable<GraphQLError> graphQLError)
        => graphQLError.Select(error => error.ConvertToIError()).ToArray();

    // Converts a collection of ValidationFailure objects to an array of IError objects
    public static IError[] ConvertToIError(this IEnumerable<ValidationFailure> graphQLError)
        => graphQLError.Select(error => error.ConvertToIError()).ToArray();

    // Converts a GraphQLError object to an IError object
    public static IError ConvertToIError(this GraphQLError graphQLError)
    {
        var builder = ErrorBuilder.New()
            .SetMessage(graphQLError.Message);

        if (graphQLError.Path is { } errorPath)
        {
            var segments = errorPath
                .Select(segment => (object)(segment?.ToString() ?? string.Empty))
                .ToList();
            if (segments.Count > 0)
            {
                builder.SetPath(HotChocolate.Path.FromList(segments));
            }
        }

        if (graphQLError.Locations != null)
        {
            foreach (var location in graphQLError.Locations)
            {
                builder.AddLocation(new Location((int)location.Line, (int)location.Column));
            }
        }

        return builder.Build();
    }

    // Converts a ValidationFailure object to an IError object
    public static IError ConvertToIError(this ValidationFailure validationFailure)
        => ErrorBuilder.New()
            .SetMessage(validationFailure.ErrorMessage)
            .Build();
}
