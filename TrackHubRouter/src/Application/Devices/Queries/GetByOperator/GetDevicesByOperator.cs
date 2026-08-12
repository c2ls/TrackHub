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

using Ardalis.GuardClauses;
using Common.Application.Attributes;
using Common.Domain.Constants;
using TrackHub.Router.Domain.Extensions;
using Microsoft.Extensions.Configuration;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.Devices.Queries.GetByOperator;


[Authorize(Resource = Resources.Devices, Action = Actions.Read)]
public readonly record struct GetDevicesByOperatorQuery(Guid OperatorId) : IRequest<IEnumerable<DeviceVm>>;

public class GetDevicesByOperatorQueryHandler(
        IConfiguration configuration,
        IOperatorReader operatorReader,
        IDeviceRegistry deviceRegistry)
        : IRequestHandler<GetDevicesByOperatorQuery, IEnumerable<DeviceVm>>
{
    private string? EncryptionKey { get; } = configuration["AppSettings:EncryptionKey"];

    // Handles the GetDevicesByOperatorQuery and returns a list of external device view models
    public async Task<IEnumerable<DeviceVm>> Handle(GetDevicesByOperatorQuery request, CancellationToken cancellationToken)
    {
        var @operator = await operatorReader.GetOperatorAsync(request.OperatorId, cancellationToken);
        if (!@operator.Enabled)
        {
            return [];
        }
        return await GetDevicesAsync(@operator, cancellationToken);
    }

    // Retrieves the devices for the operator and returns a list of external device view models
    private async Task<IEnumerable<DeviceVm>> GetDevicesAsync(
        OperatorVm @operator,
        CancellationToken cancellationToken)
    {
        var reader = deviceRegistry.GetReader((ProtocolType)@operator.ProtocolTypeId);
        return await FetchAndProcessDevicesAsync(reader, @operator, cancellationToken);
    }

    // Fetches and processes the devices using the external device reader and returns a list of external device view models
    private async Task<IEnumerable<DeviceVm>> FetchAndProcessDevicesAsync(
        IExternalDeviceReader reader,
        OperatorVm @operator,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(EncryptionKey, message: "Credential key not found.");
        if (@operator.Credential is not null)
        {
            await reader.Init(@operator.Credential.Value.Decrypt(EncryptionKey), cancellationToken);
            return await reader.GetDevicesAsync(cancellationToken);
        }
        return [];
    }
}
