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

using Common.Application.Attributes;
using Common.Domain.Constants;
using System.Runtime.CompilerServices;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Domain.Extensions;
using Microsoft.Extensions.Configuration;
using Ardalis.GuardClauses;

namespace TrackHub.Router.Application.Devices.Queries.Get;

[Authorize(Resource = Resources.Devices, Action = Actions.Read)]
[RateLimiting(PermitLimit = 3, WindowSeconds = 60)]
public readonly record struct GetDevicesQuery() : IRequest<IEnumerable<DeviceVm>>;

public class GetDevicesQueryHandler(
    IConfiguration configuration,
    IOperatorReader operatorReader,
    IOperatorSystemReader operatorSystemReader,
    IDeviceRegistry deviceRegistry,
    IDeviceTransporterReader deviceReader)
    : IRequestHandler<GetDevicesQuery, IEnumerable<DeviceVm>>
{

    private string? EncryptionKey { get; } = configuration["AppSettings:EncryptionKey"];

    public async Task<IEnumerable<DeviceVm>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
    {
        // Visibility under the caller's identity; credentials from a service-identity read of the same
        // accounts, matched back to the visible set.
        var visibleOperators = (await operatorReader.GetOperatorsAsync(cancellationToken)).ToList();
        if (visibleOperators.Count == 0)
        {
            return [];
        }

        var visibleIds = visibleOperators.Select(o => o.OperatorId).ToHashSet();
        var withCredentials = new List<OperatorVm>(visibleOperators.Count);
        foreach (var accountId in visibleOperators.Select(o => o.AccountId).Where(id => id != Guid.Empty).Distinct())
        {
            var accountOperators = await operatorSystemReader.GetOperatorsByAccountsAsync(accountId, cancellationToken);
            withCredentials.AddRange(accountOperators.Where(o => visibleIds.Contains(o.OperatorId)));
        }

        var operators = withCredentials.Where(o => o.Enabled).ToList();
        if (operators.Count == 0)
        {
            return [];
        }
        var protocols = operators.Select(o => (ProtocolType)o.ProtocolTypeId).Distinct();

        var allDevices = new List<DeviceVm>();
        await foreach (var devicesCollection in GetDevicesAsync(operators, protocols, cancellationToken))
        {
            allDevices.AddRange(devicesCollection);
        }

        return allDevices;
    }

    private async IAsyncEnumerable<IEnumerable<DeviceVm>> GetDevicesAsync(
        IEnumerable<OperatorVm> operators,
        IEnumerable<ProtocolType> protocols,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var tasks = deviceRegistry.GetReaders(protocols)
            .Select(reader
                => FetchAndProcessDevicesAsync(reader, operators, cancellationToken));
        var fetchTasks = Task.WhenAll(tasks);

        var results = await fetchTasks;
        foreach (var devices in results)
        {
            if (devices.Any())
            {
                yield return devices;
            }
        }
    }

    private async Task<IEnumerable<DeviceVm>> FetchAndProcessDevicesAsync(
        IExternalDeviceReader reader,
        IEnumerable<OperatorVm> operators,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(EncryptionKey, message: "Credential key not found.");
        var @operator = operators.FirstOrDefault(o => (ProtocolType)o.ProtocolTypeId == reader.Protocol);
        if (@operator.Credential is not null)
        {
            await reader.Init(@operator.Credential.Value.Decrypt(EncryptionKey), cancellationToken);
            var devices = await deviceReader.GetVisibleDeviceTransportersByOperatorAsync(@operator.OperatorId, cancellationToken);
            return await reader.GetDevicesAsync(devices, cancellationToken);
        }
        return [];
    }
}
