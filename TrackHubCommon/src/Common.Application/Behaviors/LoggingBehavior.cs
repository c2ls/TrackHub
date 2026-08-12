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

using System.Diagnostics;
using Common.Application.Interfaces;
using Common.Mediator;
using Microsoft.Extensions.Logging;

namespace Common.Application.Behaviors;

/// <summary>
/// Pipeline behavior that logs the full lifecycle of every MediatR request at Debug level.
/// Captures request start, completion (with elapsed time), and failure — so all transactions
/// are visible when Debug logging is enabled, without requiring try-catch in business logic.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class LoggingBehavior<TRequest, TResponse>(
    ILogger<TRequest> logger,
    IUser user,
    IIdentityService identityService
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;
        var userId = user.Id ?? string.Empty;
        string? userName = string.Empty;

        // Resolve the display name only when Debug logging is on (it costs an identity round-trip)
        // and only for user principals: service-client tokens carry a non-Guid subject (the client
        // id), which must not fail the request pipeline.
        if (logger.IsEnabled(LogLevel.Debug) && Guid.TryParse(userId, out var userGuid))
        {
            userName = await identityService.GetUserNameAsync(userGuid, cancellationToken);
        }

        logger.LogDebug("TrackHub Request Starting: {Name} by {UserId} ({UserName})",
            requestName, userId, userName);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next();
            stopwatch.Stop();

            logger.LogDebug("TrackHub Request Completed: {Name} by {UserId} in {ElapsedMilliseconds}ms",
                requestName, userId, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogDebug("TrackHub Request Failed: {Name} by {UserId} in {ElapsedMilliseconds}ms - {ExceptionType}: {ExceptionMessage}",
                requestName, userId, stopwatch.ElapsedMilliseconds, ex.GetType().Name, ex.Message);

            throw;
        }
    }
}
