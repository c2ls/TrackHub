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

using Common.Application.Exceptions;
using Common.Mediator;
using HotChocolate;

namespace Common.Application.Behaviors;

// The GraphQLValidationBehavior class is a pipeline behavior that performs validation on GraphQL requests.
// It checks if there are any validators registered and if so, it executes them to validate the request.
// If any validation failures occur, it throws a GraphQLException.
public class GraphQLValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // This method handles the request by executing the validation logic.
    // It checks if there are any validators registered and if so, it executes them to validate the request.
    // If any validation failures occur, it throws a GraphQLException.
    // Otherwise, it passes the request to the next handler in the pipeline.
    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                validators.Select(v =>
                    v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Count != 0)
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Count != 0)
                throw new GraphQLException(failures.ConvertToIError());
        }
        return await next();
    }
}
