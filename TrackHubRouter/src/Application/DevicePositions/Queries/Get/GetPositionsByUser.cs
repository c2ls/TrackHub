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

using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Common.Application.Attributes;
using Common.Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Domain.Extensions;

namespace TrackHub.Router.Application.DevicePositions.Queries.Get;

// The account-configurable map refresh has a 10-second floor, i.e. up
// to 6 legitimate calls per minute per user, plus page loads and tab returns on top.
// 12/min accommodates that legal cadence; provider protection remains the cached
// projection and the per-operator fallback, not this limiter.
[Authorize(Resource = Resources.Positions, Action = Actions.Read)]
[RateLimiting(PermitLimit = 12, WindowSeconds = 60)]
public readonly record struct GetPositionsByUserQuery() : IRequest<IEnumerable<PositionVm>>;

public class GetPositionsByUserQueryHandler(
        IConfiguration configuration,
        Application.Gating.IAccountModeResolver modeResolver,
        IOperatorReader operatorReader,
        IPositionRegistry positionRegistry,
        IDeviceTransporterReader deviceReader,
        ITransporterPositionReader transporterPositionReader,
        IPositionSystemWriter positionSystemWriter,
        ILogger<GetPositionsByUserQueryHandler> logger)
        : IRequestHandler<GetPositionsByUserQuery, IEnumerable<PositionVm>>
{
    private string? EncryptionKey { get; } = configuration["AppSettings:EncryptionKey"];

    /// <summary>
    /// Retrieves the operators, protocols, and device positions asynchronously
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the collection of PositionVm</returns>
    public async Task<IEnumerable<PositionVm>> Handle(GetPositionsByUserQuery request, CancellationToken cancellationToken)
    {
        var allOperators = await operatorReader.GetOperatorsAsync(cancellationToken);
        var operators = allOperators.ToList();
        if (operators.Count == 0)
        {
            return [];
        }

        // Mode split per account, resolved through the single IAccountModeResolver
        //:
        // - gps.integration DISABLED  -> on-demand: read the GPS provider directly, persist
        //   what was read, and fall back to the stored projection if the provider fails.
        // - gps.integration ENABLED   -> always serve the stored latest-position projection;
        //   the background SyncWorker keeps it current, so the provider is never contacted here.
        var integrationEnabledAccounts = new HashSet<Guid>();
        foreach (var accountId in operators.Select(o => o.AccountId).Where(id => id != Guid.Empty).Distinct())
        {
            if (await modeResolver.IsIntegrationEnabledAsync(accountId, cancellationToken))
            {
                integrationEnabledAccounts.Add(accountId);
            }
        }
        var onDemandOperators = operators
            .Where(o => Application.Gating.GpsFeatureGate.CanReadProviderOnDemand(o, integrationEnabledAccounts))
            .ToList();
        var storedProjectionOperators = operators.Except(onDemandOperators).ToList();

        var allPositions = new List<PositionVm>();
        if (onDemandOperators.Count > 0)
        {
            var protocols = onDemandOperators.Select(o => (ProtocolType)o.ProtocolTypeId).Distinct();
            await foreach (var positionsCollection in GetDevicePositionAsync(onDemandOperators, protocols, cancellationToken))
            {
                allPositions.AddRange(positionsCollection);
            }
        }

        if (storedProjectionOperators.Count > 0)
        {
            // One batched Telemetry read for every stored-projection operator (this path runs
            // on every map refresh — previously one call per operator).
            try
            {
                allPositions.AddRange(await transporterPositionReader.GetTransporterPositionsAsync(
                    storedProjectionOperators.Select(o => o.OperatorId).ToArray(), cancellationToken));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error retrieving stored positions for {OperatorCount} operators", storedProjectionOperators.Count);
            }
        }

        //Most recent position for each transporter if multiple positions are available
        var result = allPositions
            .GroupBy(p => p.TransporterId)
            .Select(g => g.OrderByDescending(p => p.DeviceDateTime).First())
            .ToList();

        // Diagnosability: an empty map for an account that actually has active
        // assignments is the known defect signature. Name the branch that produced nothing so it is
        // never silent. No contract change — empty remains a valid response.
        if (result.Count == 0 && operators.Any(o => o.Enabled))
        {
            await LogEmptyMapDiagnosticsAsync(operators, onDemandOperators, cancellationToken);
        }

        return result;
    }

    // Emits a structured warning per enabled operator that has active assignments in the account yet
    // contributed no positions, naming the branch (provider read + stored fallback, or stored
    // projection / visibility narrowing).
    private async Task LogEmptyMapDiagnosticsAsync(
        IReadOnlyCollection<OperatorVm> operators,
        IReadOnlyCollection<OperatorVm> onDemandOperators,
        CancellationToken cancellationToken)
    {
        foreach (var @operator in operators.Where(o => o.Enabled))
        {
            IEnumerable<DeviceTransporterVm> assigned;
            try
            {
                // Account-wide, group-independent catalog: proves the account HAS active assignments.
                assigned = await deviceReader.GetDeviceTransporterAsync(@operator.AccountId, @operator.OperatorId, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Live map empty and the diagnostic catalog read failed for operator {OperatorId} (account {AccountId}).",
                    @operator.OperatorId, @operator.AccountId);
                continue;
            }

            var assignedCount = assigned.Count();
            if (assignedCount == 0)
            {
                continue;
            }

            var branch = onDemandOperators.Contains(@operator)
                ? "provider read + stored fallback returned nothing"
                : "stored projection empty or visibility narrowing (user not in the transporter's group)";
            logger.LogWarning(
                "Live map returned no positions for operator {OperatorId} (account {AccountId}) despite {AssignedCount} active assignment(s); empty branch: {Branch}.",
                @operator.OperatorId, @operator.AccountId, assignedCount, branch);
        }
    }

    /// <summary>
    /// Retrieves the device positions asynchronously
    /// </summary>
    /// <param name="operators"></param>
    /// <param name="protocols"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>returns the collection of PositionVm</returns>
    private async IAsyncEnumerable<IEnumerable<PositionVm>> GetDevicePositionAsync(
        IEnumerable<OperatorVm> operators,
        IEnumerable<ProtocolType> protocols,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var tasks = positionRegistry.GetReaders(protocols)
            .Select(reader
                => FetchAndProcessPositionsAsync(reader, operators, cancellationToken));
        var fetchTasks = Task.WhenAll(tasks);

        var results = await fetchTasks;
        foreach (var positions in results)
        {
            if (positions.Any())
            {
                yield return positions;
            }
        }
    }

    /// <summary>
    /// Fetches and processes the positions asynchronously
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="operators"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>returns the collection of PositionVm</returns>
    private async Task<IEnumerable<PositionVm>> FetchAndProcessPositionsAsync(
        IPositionReader reader,
        IEnumerable<OperatorVm> operators,
        CancellationToken cancellationToken)
    {
        // Several operators may share the same protocol (e.g. two providers of the same brand);
        // query each sequentially with its own credential and device catalog.
        var positions = new List<PositionVm>();
        foreach (var @operator in operators.Where(o => (ProtocolType)o.ProtocolTypeId == reader.Protocol))
        {
            positions.AddRange(await TryGetPositionsAsync(reader, @operator, cancellationToken));
        }
        return positions;
    }

    private async Task<IEnumerable<PositionVm>> TryGetPositionsAsync(
        IPositionReader reader,
        OperatorVm @operator,
        CancellationToken cancellationToken)
    {
        try
        {
            if (@operator.Credential is null)
                throw new ArgumentNullException(nameof(@operator), "Credential is null");
            Guard.Against.Null(EncryptionKey, message: "Credential key not found.");

            var devices = await deviceReader.GetVisibleDeviceTransportersByOperatorAsync(@operator.OperatorId, cancellationToken);
            var credential = @operator.Credential.Value.Decrypt(EncryptionKey);
            await reader.Init(credential, cancellationToken);
            var positions = await reader.GetDevicePositionAsync(devices, cancellationToken);
            var positionsList = positions.ToList();
            if (positionsList.Count > 0)
            {
                // On-demand mode: the account has no background sync, so the Router API keeps
                // the latest-position projection current with what it just read from the provider.
                await PersistLatestPositionsAsync(positionsList, cancellationToken);
                return positionsList;
            }
            return await GetStoredPositionsAsync(@operator.OperatorId, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error retrieving positions for user");
            return await GetStoredPositionsAsync(@operator.OperatorId, cancellationToken);
        }
    }

    // Reads the stored latest-position projection (user-group-scoped in Manager). This is
    // the PRIMARY read for integration-enabled accounts and the FALLBACK when an on-demand
    // provider read fails or returns nothing.
    private async Task<IEnumerable<PositionVm>> GetStoredPositionsAsync(
        Guid operatorId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await transporterPositionReader.GetTransporterPositionAsync(operatorId, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error retrieving stored positions for operator {OperatorId}", operatorId);
            return [];
        }
    }

    /// <summary>
    /// Best-effort upsert of provider-read positions using the Router's service identity.
    /// Failures are logged and never block the map read.
    /// </summary>
    private async Task PersistLatestPositionsAsync(
        IReadOnlyCollection<PositionVm> positions,
        CancellationToken cancellationToken)
    {
        var validPositions = positions.Where(IsValidPosition).ToArray();
        if (validPositions.Length == 0)
        {
            return;
        }

        try
        {
            await positionSystemWriter.AddOrUpdatePositionAsync(validPositions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to persist on-demand positions; the map read is unaffected.");
        }
    }

    private static bool IsValidPosition(PositionVm position)
        => position.TransporterId != Guid.Empty
           && position.DeviceDateTime != default
           && position.Latitude is >= -90d and <= 90d
           && position.Longitude is >= -180d and <= 180d;

}
