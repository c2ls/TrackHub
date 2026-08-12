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
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Common.Web.Infrastructure;

// CustomExceptionHandler class implements the IExceptionHandler interface
// to handle exceptions and return appropriate HTTP responses.
public class CustomExceptionHandler : IExceptionHandler
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;

    public CustomExceptionHandler()
    {
        // Register known exception types and their corresponding handlers.
        _exceptionHandlers = new()
            {
                { typeof(ValidationException), HandleValidationException },
                { typeof(NotFoundException), HandleNotFoundException },
                { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
                { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
                { typeof(TooManyRequestsException), HandleTooManyRequestsException },
                { typeof(ConflictException), HandleConflictException },
                { typeof(AccountSuspendedException), HandleAccountSuspendedException },
            };
    }

    // TryHandleAsync method attempts to handle the exception by invoking the appropriate handler based on the exception type.
    // It returns true if the exception was handled successfully, otherwise false.
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        if (_exceptionHandlers.TryGetValue(exceptionType, out Func<HttpContext, Exception, Task>? value))
        {
            await value.Invoke(httpContext, exception);
            return true;
        }

        // Fallback: return 500 for any unhandled exception type
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        }, cancellationToken);

        return true;
    }

    // HandleConflictException handles ConflictException by setting the response status code to 409 (Conflict).
    private async Task HandleConflictException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = ex.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
        });
    }

    // HandleAccountSuspendedException handles AccountSuspendedException by setting the response status code to 403 (Forbidden).
    private async Task HandleAccountSuspendedException(HttpContext httpContext, Exception ex)
    {
        var exception = (AccountSuspendedException)ex;
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Account not operational",
            Detail = exception.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        });
    }

    // HandleValidationException method handles the ValidationException by setting the response status code to 400 (Bad Request)
    // and returning a ValidationProblemDetails object as JSON response.
    private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        var exception = (ValidationException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(exception.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        });
    }

    // HandleNotFoundException method handles the NotFoundException by setting the response status code to 404 (Not Found)
    // and returning a ProblemDetails object as JSON response.
    private async Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    {
        var exception = (NotFoundException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails()
        {
            Status = StatusCodes.Status404NotFound,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The specified resource was not found.",
            Detail = exception.Message
        });
    }

    // HandleUnauthorizedAccessException method handles the UnauthorizedAccessException by setting the response status code to 401 (Unauthorized)
    // and returning a ProblemDetails object as JSON response.
    private async Task HandleUnauthorizedAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        });
    }

    // HandleForbiddenAccessException method handles the ForbiddenAccessException by setting the response status code to 403 (Forbidden)
    // and returning a ProblemDetails object as JSON response.
    private async Task HandleForbiddenAccessException(HttpContext httpContext, Exception ex)
    {
        var exception = (ForbiddenAccessException)ex;
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = exception.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        });
    }

    // HandleTooManyRequestsException method handles the TooManyRequestsException by setting the response status code to 429 (Too Many Requests)
    // and returning a ProblemDetails object as JSON response with Retry-After header.
    private async Task HandleTooManyRequestsException(HttpContext httpContext, Exception ex)
    {
        var exception = (TooManyRequestsException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (exception.RetryAfterSeconds.HasValue)
        {
            httpContext.Response.Headers.Append("Retry-After", exception.RetryAfterSeconds.Value.ToString());
        }

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too Many Requests",
            Type = "https://tools.ietf.org/html/rfc6585#section-4",
            Detail = exception.Message
        });
    }
}
