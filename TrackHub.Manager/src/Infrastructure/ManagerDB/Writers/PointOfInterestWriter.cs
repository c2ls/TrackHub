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

using System.Text.Json;
using Common.Application.Interfaces;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class PointOfInterestWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IPointOfInterestWriter
{
    public async Task<PointOfInterestVm> CreatePointOfInterestAsync(PointOfInterestDto pointOfInterestDto, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountWriteAccess(pointOfInterestDto.AccountId);
        await RequireGroupInAccountAsync(pointOfInterestDto.GroupId, accountId, cancellationToken);

        var pointOfInterest = new PointOfInterest(
            pointOfInterestDto.Name,
            pointOfInterestDto.Description,
            pointOfInterestDto.Type,
            pointOfInterestDto.Latitude,
            pointOfInterestDto.Longitude,
            pointOfInterestDto.Address,
            pointOfInterestDto.Color,
            pointOfInterestDto.GroupId,
            pointOfInterestDto.Active,
            accountId);

        await Context.PointsOfInterest.AddAsync(pointOfInterest, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        return new PointOfInterestVm(
            pointOfInterest.PointOfInterestId,
            pointOfInterest.AccountId,
            pointOfInterest.Name,
            pointOfInterest.Description,
            pointOfInterest.Type,
            pointOfInterest.Latitude,
            pointOfInterest.Longitude,
            pointOfInterest.Address,
            pointOfInterest.Color,
            pointOfInterest.GroupId,
            pointOfInterest.Active);
    }

    public async Task UpdatePointOfInterestAsync(Guid pointOfInterestId, UpdatePointOfInterestDto pointOfInterestDto, CancellationToken cancellationToken)
    {
        var pointOfInterest = await Context.PointsOfInterest.FindAsync([pointOfInterestId], cancellationToken)
            ?? throw new NotFoundException(nameof(PointOfInterest), $"{pointOfInterestId}");

        var accountId = RequireAccountWriteAccess(pointOfInterest.AccountId);
        await RequireGroupInAccountAsync(pointOfInterestDto.GroupId, accountId, cancellationToken);

        Context.PointsOfInterest.Attach(pointOfInterest);

        pointOfInterest.Name = pointOfInterestDto.Name;
        pointOfInterest.Description = pointOfInterestDto.Description;
        pointOfInterest.Type = pointOfInterestDto.Type;
        pointOfInterest.Latitude = pointOfInterestDto.Latitude;
        pointOfInterest.Longitude = pointOfInterestDto.Longitude;
        pointOfInterest.Address = pointOfInterestDto.Address;
        pointOfInterest.Color = pointOfInterestDto.Color;
        pointOfInterest.GroupId = pointOfInterestDto.GroupId;
        pointOfInterest.Active = pointOfInterestDto.Active;

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePointOfInterestAsync(Guid pointOfInterestId, CancellationToken cancellationToken)
    {
        var pointOfInterest = await Context.PointsOfInterest.FindAsync([pointOfInterestId], cancellationToken)
            ?? throw new NotFoundException(nameof(PointOfInterest), $"{pointOfInterestId}");

        var accountId = RequireAccountWriteAccess(pointOfInterest.AccountId);

        Context.PointsOfInterest.Attach(pointOfInterest);
        Context.PointsOfInterest.Remove(pointOfInterest);

        AddAuditEvent(
            accountId,
            "Delete",
            nameof(PointOfInterest),
            pointOfInterestId.ToString(),
            JsonSerializer.Serialize(new { pointOfInterest.Name, pointOfInterest.Type, pointOfInterest.Latitude, pointOfInterest.Longitude, pointOfInterest.GroupId, pointOfInterest.Active }),
            null);

        await Context.SaveChangesAsync(cancellationToken);
    }

    private async Task RequireGroupInAccountAsync(long? groupId, Guid accountId, CancellationToken cancellationToken)
    {
        if (!groupId.HasValue)
        {
            return;
        }

        var groupBelongs = await Context.Groups
            .AnyAsync(g => g.GroupId == groupId.Value && g.AccountId == accountId, cancellationToken);

        if (!groupBelongs)
        {
            throw new NotFoundException(nameof(Group), $"{groupId.Value}");
        }
    }
}
