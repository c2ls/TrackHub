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

namespace TrackHub.TripManagement.Infrastructure.TripDB.Readers;

public sealed class RoutePlanReader(IApplicationDbContext context) : IRoutePlanReader
{
    public async Task<RoutePlanVm?> GetPlanAsync(Guid routePlanId, Guid accountId, CancellationToken cancellationToken)
    {
        var plan = await context.RoutePlans
            .FirstOrDefaultAsync(p => p.RoutePlanId == routePlanId && p.AccountId == accountId, cancellationToken);

        return plan is null ? null : TripMapper.ToVm(plan);
    }

    public async Task<RoutePlanVm?> GetPlanForTripAsync(Guid tripId, Guid accountId, CancellationToken cancellationToken)
    {
        var plan = await context.RoutePlans
            .Where(p => p.TripId == tripId && p.AccountId == accountId)
            .OrderByDescending(p => p.ComputedAt)
            .ThenByDescending(p => p.Created)
            .FirstOrDefaultAsync(cancellationToken);

        return plan is null ? null : TripMapper.ToVm(plan);
    }

    public async Task<IReadOnlyCollection<CoordinateVm>> GetPlanGeometryAsync(Guid routePlanId, Guid accountId, CancellationToken cancellationToken)
    {
        var plan = await context.RoutePlans
            .FirstOrDefaultAsync(p => p.RoutePlanId == routePlanId && p.AccountId == accountId, cancellationToken);

        if (plan?.Geom is null)
        {
            return [];
        }

        return [.. plan.Geom.Coordinates.Select(c => new CoordinateVm(c.Y, c.X))];
    }
}
