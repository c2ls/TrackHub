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
using Ardalis.GuardClauses;
using Common.Application.Attributes;
using Common.Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrackHub.Router.Domain.Extensions;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.PingOperator.Queries;

[Authorize(Resource = Resources.Credentials, Action = Actions.Custom)]
public readonly record struct PingOperatorQuery(Guid OperatorId) : IRequest<bool>;

// This class handles the PingOperatorQuery and implements the IRequestHandler interface
public class PingOperatorQueryHandler(
    IConfiguration configuration,
    IOperatorReader operatorReader,
    IConnectivityRegistry connectivityRegistry,
    IOperatorHealthCheckSystemWriter healthWriter,
    ILogger<PingOperatorQueryHandler> logger)
    : IRequestHandler<PingOperatorQuery, bool>
{
    // This property retrieves the EncryptionKey from the configuration
    private string? EncryptionKey { get; } = configuration["AppSettings:EncryptionKey"];

    // This method handles the PingOperatorQuery and returns a boolean indicating the success of the operation
    public async Task<bool> Handle(PingOperatorQuery request, CancellationToken cancellationToken)
    {
        // Get the operator details using the operatorReader
        var @operator = await operatorReader.GetOperatorAsync(request.OperatorId, cancellationToken);
        if (!@operator.Enabled)
        {
            return false;
        }
        // Ping the operator asynchronously
        return await PingAsync(@operator, cancellationToken);
    }

    // This method pings the operator by testing the connectivity
    private async Task<bool> PingAsync(
        OperatorVm @operator,
        CancellationToken cancellationToken)
    {
        // Get the connectivity tester based on the protocol type of the operator
        var reader = connectivityRegistry.GetTester((ProtocolType)@operator.ProtocolTypeId);
        // Test the connectivity asynchronously
        return await TestConnectivityAsync(reader, @operator, cancellationToken);
    }

    // Tests connectivity and persists the manually triggered result as an operator
    // health check. Health is core: the record is written with the Router's own service
    // identity regardless of the account's feature flags, and a persistence failure
    // never masks the connectivity result.
    private async Task<bool> TestConnectivityAsync(
        IConnectivityTester reader,
        OperatorVm @operator,
        CancellationToken cancellationToken)
    {
        // Ensure that the EncryptionKey is not null, otherwise throw an exception
        Guard.Against.Null(EncryptionKey, message: "Credential key not found.");
        if (@operator.Credential is null)
        {
            return false;
        }

        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await reader.Ping(@operator.Credential.Value.Decrypt(EncryptionKey), cancellationToken);
            stopwatch.Stop();
            await RecordAsync(@operator, startedAt, (int)stopwatch.ElapsedMilliseconds, "HEALTHY", null, null, cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            await RecordAsync(@operator, startedAt, (int)stopwatch.ElapsedMilliseconds, "OFFLINE", ex.GetType().Name, ex.Message, cancellationToken);
            throw;
        }
    }

    private async Task RecordAsync(
        OperatorVm @operator,
        DateTimeOffset startedAt,
        int latencyMs,
        string status,
        string? errorCode,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            await healthWriter.RecordAsync(new OperatorHealthCheckDto(
                AccountId: @operator.AccountId,
                OperatorId: @operator.OperatorId,
                CheckType: "MANUAL",
                Status: status,
                LatencyMs: latencyMs,
                StartedAt: startedAt,
                CompletedAt: DateTimeOffset.UtcNow,
                ErrorCode: errorCode,
                ErrorMessage: errorMessage,
                RetryCount: 0,
                CorrelationId: Guid.NewGuid().ToString()), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to persist manual health check for operator {OperatorId}.", @operator.OperatorId);
        }
    }
}
