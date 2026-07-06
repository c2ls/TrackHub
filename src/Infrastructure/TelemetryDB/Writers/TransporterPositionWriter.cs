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

using TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Writers;

public sealed class TransporterPositionWriter(IApplicationDbContext context) : ITransporterPositionWriter
{

    /// <summary>
    /// Bulk insert transporter positions
    /// </summary>
    /// <param name="positionsDto"></param>
    /// <param name="cancellationToken">Task</param>
    /// <returns></returns>
    public async Task BulkTransporterPositionAsync(IEnumerable<TransporterPositionDto> positionsDto, CancellationToken cancellationToken)
    {
        var incoming = positionsDto.ToList();
        if (incoming.Count == 0)
        {
            return;
        }

        var transporterIds = incoming.Select(p => p.TransporterId).Distinct().ToArray();
        var existingByTransporter = await context.TransporterPositions
            .Where(p => transporterIds.Contains(p.TransporterId))
            .ToDictionaryAsync(p => p.TransporterId, cancellationToken);

        foreach (var positionDto in incoming)
        {
            if (existingByTransporter.TryGetValue(positionDto.TransporterId, out var existing))
            {
                if (existing.DateTime >= positionDto.DeviceDateTime.UtcDateTime)
                {
                    continue;
                }

                Apply(existing, positionDto);
                continue;
            }

            var created = new TransporterPosition(
                positionDto.TransporterId,
                positionDto.GeometryId,
                positionDto.Latitude,
                positionDto.Longitude,
                positionDto.Altitude,
                positionDto.DeviceDateTime.UtcDateTime,
                positionDto.DeviceDateTime.Offset,
                positionDto.Speed,
                positionDto.Course,
                positionDto.EventId,
                positionDto.Address,
                positionDto.City,
                positionDto.State,
                positionDto.Country,
                MapAttributes(positionDto.Attributes));

            await context.TransporterPositions.AddAsync(created, cancellationToken);
            existingByTransporter[positionDto.TransporterId] = created;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// UpdateTransporterPositionAsync method is used to update the existing TransporterPosition in the database
    /// </summary>
    /// <param name="positionDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Task</returns>
    /// <exception cref="NotFoundException">if the transporter position is not found</exception>
    public async Task UpdateTransporterPositionAsync(TransporterPositionDto positionDto, CancellationToken cancellationToken)
    {
        var position = await context.TransporterPositions.FirstOrDefaultAsync(t => t.TransporterId == positionDto.TransporterId, cancellationToken)
            ?? throw new NotFoundException(nameof(TransporterPosition), $"{positionDto.TransporterId}");

        context.TransporterPositions.Attach(position);

        position.GeometryId = positionDto.GeometryId;
        position.Latitude = positionDto.Latitude;
        position.Longitude = positionDto.Longitude;
        position.Altitude = positionDto.Altitude;
        position.DateTime = positionDto.DeviceDateTime.UtcDateTime;
        position.Offset = positionDto.DeviceDateTime.Offset;
        position.Speed = positionDto.Speed;
        position.Course = positionDto.Course;
        position.EventId = positionDto.EventId;
        position.Address = positionDto.Address;
        position.City = positionDto.City;
        position.State = positionDto.State;
        position.Country = positionDto.Country;
        position.Attributes = positionDto.Attributes == null ? null : new AttributesVm(
            positionDto.Attributes?.Ignition,
            positionDto.Attributes?.Satellites,
            positionDto.Attributes?.Mileage,
            positionDto.Attributes?.Hourmeter,
            positionDto.Attributes?.Temperature
        );

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void Apply(TransporterPosition position, TransporterPositionDto positionDto)
    {
        position.GeometryId = positionDto.GeometryId;
        position.Latitude = positionDto.Latitude;
        position.Longitude = positionDto.Longitude;
        position.Altitude = positionDto.Altitude;
        position.DateTime = positionDto.DeviceDateTime.UtcDateTime;
        position.Offset = positionDto.DeviceDateTime.Offset;
        position.Speed = positionDto.Speed;
        position.Course = positionDto.Course;
        position.EventId = positionDto.EventId;
        position.Address = positionDto.Address;
        position.City = positionDto.City;
        position.State = positionDto.State;
        position.Country = positionDto.Country;
        position.Attributes = MapAttributes(positionDto.Attributes);
    }

    private static AttributesVm? MapAttributes(AttributesDto? attributes)
        => attributes == null ? null : new AttributesVm(
            attributes.Value.Ignition,
            attributes.Value.Satellites,
            attributes.Value.Mileage,
            attributes.Value.Hourmeter,
            attributes.Value.Temperature);

    /// <summary>
    /// This method will delete an existing TransporterPosition in the database
    /// </summary>
    /// <param name="transporterId">The TransporterPosition identifier</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    public async Task DeleteTransporterPositionAsync(Guid transporterId, CancellationToken cancellationToken)
    {
        var position = await context.TransporterPositions.FirstOrDefaultAsync(t => t.TransporterId == transporterId, cancellationToken);

        if (position is not null)
        {
            context.TransporterPositions.Attach(position);

            context.TransporterPositions.Remove(position);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
