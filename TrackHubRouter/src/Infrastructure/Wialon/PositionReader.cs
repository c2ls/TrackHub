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

using TrackHub.Router.Infrastructure.Wialon.Mappers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Operator;

namespace TrackHub.Router.Infrastructure.Wialon;

/// <summary>
/// Reader for retrieving position information from Wialon API.
/// </summary>
public sealed class PositionReader(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    IProviderSessionStore sessionStore)
    : WialonReaderBase(httpClientFactory, httpClientService, sessionStore), IPositionReader
{
    // Wialon search flags
    private const int FlagBasicInfo = 1;       // Basic unit info (id, name)
    private const int FlagPosition = 1024;     // Last position data
    private const int FlagsWithPosition = FlagBasicInfo | FlagPosition; // Combined flags (1025)

    // Message flags for historical data
    private const int MessageFlagPosition = 1;      // Messages with position
    private const int MessageFlagMask = 65281;      // Filter mask for position messages

    /// <summary>
    /// Retrieves the last position of a single device asynchronously.
    /// </summary>
    public async Task<PositionVm> GetDevicePositionAsync(DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var parameters = new
        {
            id = deviceDto.Identifier,
            flags = FlagsWithPosition
        };

        var result = await PostAsync<SingleItemResponse>("core/search_item", parameters, cancellationToken);
        return result?.Item is null
            ? throw new InvalidOperationException($"Device not found: {deviceDto.Identifier}")
            : result.Item.Value.MapToPositionVm(deviceDto);
    }

    /// <summary>
    /// Retrieves the last positions of multiple devices asynchronously.
    /// </summary>
    public async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(IEnumerable<DeviceTransporterVm> devices, CancellationToken cancellationToken)
    {
        var parameters = new
        {
            spec = new
            {
                itemsType = "avl_unit",
                propName = "sys_name",
                propValueMask = "*",
                sortType = "sys_name"
            },
            force = 1,
            flags = FlagsWithPosition,
            from = 0,
            to = 0
        };

        var result = await PostAsync<SearchResponse>("core/search_items", parameters, cancellationToken);
        if (result?.Items is null || !result.Items.Any())
        {
            return [];
        }

        var devicesDictionary = devices.ToDictionary(device => device.Identifier, device => device);
        return result.Items.MapToPositionVm(devicesDictionary).Distinct();
    }

    /// <summary>
    /// Retrieves the positions of a device within a specified time range asynchronously.
    /// Uses messages/load_interval to get historical position data.
    /// </summary>
    public async Task<IEnumerable<PositionVm>> GetPositionAsync(DateTimeOffset from, DateTimeOffset to, DeviceTransporterVm deviceDto, CancellationToken cancellationToken)
    {
        var timeFrom = from.ToUnixTimeSeconds();
        var timeTo = to.ToUnixTimeSeconds();

        var parameters = new
        {
            itemId = deviceDto.Identifier,
            timeFrom,
            timeTo,
            flags = MessageFlagPosition,
            flagsMask = MessageFlagMask,
            loadCount = 0xFFFFFFFF  // Load all messages in range
        };

        var result = await PostAsync<MessageResponse>("messages/load_interval", parameters, cancellationToken);
        return result?.Messages is null ? [] : result.Messages.MapToPositionVm(deviceDto);
    }
}
