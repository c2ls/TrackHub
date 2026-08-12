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
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Domain.Extensions;

namespace TrackHub.Router.Application.Positions;

public abstract class PositionBaseHandler
{
    /// <summary>
    /// Enforces the transporter group-visibility rule for user principals before any
    /// provider or history read: non-privileged users must share a group with the
    /// target transporter. Administrators/managers and service clients pass through.
    /// </summary>
    protected static async Task EnsureTransporterVisibilityAsync(
        IGroupVisibilityReader groupVisibilityReader,
        ICurrentPrincipal principal,
        Guid accountId,
        Guid transporterId,
        CancellationToken cancellationToken)
    {
        if (principal.PrincipalType != PrincipalType.User || !principal.UserId.HasValue)
        {
            return;
        }

        if (string.Equals(principal.Role, Roles.Administrator, StringComparison.OrdinalIgnoreCase)
            || string.Equals(principal.Role, Roles.Manager, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var visible = await groupVisibilityReader.ValidateGroupVisibilityAsync(
            accountId,
            principal.UserId.Value,
            "Transporter",
            transporterId.ToString(),
            cancellationToken);

        if (!visible)
        {
            throw new ForbiddenAccessException($"Transporter {transporterId} is not visible to the requesting user.");
        }
    }

    /// <summary>
    /// Retrieves the device positions asynchronously
    /// </summary>
    /// <param name="operator"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>returns the collection of PositionVm</returns>
    protected async Task<IEnumerable<PositionVm>> GetDevicePositionAsync(
        IPositionRegistry positionRegistry,
        string encryptionKey,
        OperatorVm @operator,
        DateTimeOffset from,
        DateTimeOffset to,
        DeviceTransporterVm device,
        CancellationToken cancellationToken)
    {
        var reader = positionRegistry.GetReader((ProtocolType)@operator.ProtocolTypeId);
        if (@operator.Credential is not null)
        {
            await reader.Init(@operator.Credential.Value.Decrypt(encryptionKey), cancellationToken);
            var positions = await reader.GetPositionAsync(from, to, device, cancellationToken);
            return positions;
        }
        return [];
    }
}
