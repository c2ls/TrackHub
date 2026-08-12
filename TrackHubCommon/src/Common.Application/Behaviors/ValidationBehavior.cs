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

using Common.Mediator;
using ValidationException = Common.Application.Exceptions.ValidationException;

namespace Common.Application.Behaviors;

// Represents a pipeline behavior for request validation.
// It performs validation on the incoming request using a collection of validators.
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // Handles the request by performing validation and invoking the next behavior in the pipeline.
    // If validation fails, a ValidationException is thrown.
    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        // Check if there are any validators
        if (validators.Any())
        {
            // Create a validation context for the request
            var context = new ValidationContext<TRequest>(request);

            // Perform validation asynchronously using all the validators
            var validationResults = await Task.WhenAll(
                validators.Select(v =>
                    v.ValidateAsync(context, cancellationToken)));

            // Get all the validation failures
            var failures = validationResults
                .Where(r => r.Errors.Count != 0)
                .SelectMany(r => r.Errors)
                .ToList();

            // If there are validation failures, throw a ValidationException
            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        // Invoke the next behavior in the pipeline
        return await next();
    }
}
