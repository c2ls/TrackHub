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

namespace Common.Application.Exceptions;

/// <summary>
/// Represents an exception that is thrown when one or more validation failures occur.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <summary>The generic code used when a caller does not supply a specific one.</summary>
    public const string DefaultCode = "VALIDATION_ERROR";

    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
        Code = DefaultCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with the specified validation failures.
    /// </summary>
    /// <param name="failures">The validation failures.</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    /// <summary>
    /// Initializes a rejection carrying a SPECIFIC machine-readable code
    /// (<c>TRIP_NOT_ACTIVE</c>, <c>POD_DOCUMENT_NOT_CLEAN</c>, …) alongside the failures.
    /// <para>
    /// The GraphQL error filter surfaces <see cref="Code"/> as the error code. Until it did, a
    /// domain rejection thrown as a validation failure reached the client as an unmapped
    /// <c>Unexpected Execution Error</c> with no code at all, because the filter had no
    /// <see cref="ValidationException"/> branch — so the specific, localizable codes the API
    /// promises were unreachable and every per-code translation in the portal was dead.
    /// </para>
    /// </summary>
    public ValidationException(string code, IEnumerable<ValidationFailure> failures)
        : this(failures)
    {
        Code = code;
    }

    /// <summary>
    /// Gets the dictionary of validation errors.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>Machine-readable code; <see cref="DefaultCode"/> unless one was supplied.</summary>
    public string Code { get; }
}
