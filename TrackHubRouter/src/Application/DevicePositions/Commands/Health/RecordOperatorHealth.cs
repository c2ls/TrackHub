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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrackHub.Router.Domain.Extensions;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.DevicePositions.Commands.Health;

public readonly record struct RecordOperatorHealthCommand(
    OperatorVm Operator,
    string CheckType = "PING") : IRequest<bool>;

public class RecordOperatorHealthCommandHandler(
    IConfiguration configuration,
    IConnectivityRegistry connectivityRegistry,
    IOperatorHealthCheckWriter healthWriter,
    IAlertEventWriter alertWriter,
    ILogger<RecordOperatorHealthCommandHandler> logger) : IRequestHandler<RecordOperatorHealthCommand, bool>
{
    private string? EncryptionKey { get; } = configuration["AppSettings:EncryptionKey"];

    public async Task<bool> Handle(RecordOperatorHealthCommand request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(EncryptionKey, message: "Credential key not found.");
        if (request.Operator.Credential is null)
        {
            return false;
        }

        var startedAt = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();
        var status = "HEALTHY";
        string? errorCode = null;
        string? errorMessage = null;
        var previousStatus = request.Operator.HealthStatus;

        try
        {
            var tester = connectivityRegistry.GetTester((ProtocolType)request.Operator.ProtocolTypeId);
            await tester.Ping(request.Operator.Credential.Value.Decrypt(EncryptionKey), cancellationToken);
        }
        catch (Exception ex)
        {
            status = "OFFLINE";
            errorCode = ex.GetType().Name;
            errorMessage = ex.Message;
            logger.LogWarning(ex, "Operator {OperatorId} connectivity probe failed.", request.Operator.OperatorId);
        }
        finally
        {
            stopwatch.Stop();
        }

        try
        {
            await healthWriter.RecordAsync(new OperatorHealthCheckDto(
                AccountId: request.Operator.AccountId,
                OperatorId: request.Operator.OperatorId,
                CheckType: request.CheckType,
                Status: status,
                LatencyMs: (int)stopwatch.ElapsedMilliseconds,
                StartedAt: startedAt,
                CompletedAt: DateTimeOffset.UtcNow,
                ErrorCode: errorCode,
                ErrorMessage: errorMessage,
                RetryCount: 0,
                CorrelationId: correlationId), cancellationToken);

            if (status == "OFFLINE")
            {
                await alertWriter.RecordAsync(new AlertEventDto(
                    AccountId: request.Operator.AccountId,
                    EventType: "GpsOperatorOffline",
                    Severity: "Critical",
                    SourceModule: "TrackHub.Router.SyncWorker",
                    ResourceType: "Operator",
                    ResourceId: request.Operator.OperatorId.ToString(),
                    Status: "Open",
                    PayloadJson: BuildAlertPayload(errorCode, errorMessage ?? "Operator connectivity probe failed"),
                    DeduplicationKey: $"operator-offline:{request.Operator.OperatorId}"), cancellationToken);
            }
            else if (!string.Equals(previousStatus, "HEALTHY", StringComparison.OrdinalIgnoreCase)
                     && !string.IsNullOrEmpty(previousStatus))
            {
                await alertWriter.RecordAsync(new AlertEventDto(
                    AccountId: request.Operator.AccountId,
                    EventType: "GpsOperatorRecovered",
                    Severity: "Info",
                    SourceModule: "TrackHub.Router.SyncWorker",
                    ResourceType: "Operator",
                    ResourceId: request.Operator.OperatorId.ToString(),
                    Status: "Open",
                    PayloadJson: BuildAlertPayload(null, "Operator connectivity restored"),
                    DeduplicationKey: $"operator-recovered:{request.Operator.OperatorId}:{DateTimeOffset.UtcNow:yyyyMMddHHmm}"), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to record health check for operator {OperatorId}.", request.Operator.OperatorId);
        }

        return status == "HEALTHY";
    }

    private static string BuildAlertPayload(string? errorCode, string? message)
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            errorCode,
            message
        });
    }
}
