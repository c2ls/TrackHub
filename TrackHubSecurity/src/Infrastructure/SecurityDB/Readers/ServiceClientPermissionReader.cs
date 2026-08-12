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

using TrackHub.Security.Domain.Models;
using TrackHub.Security.Infrastructure.SecurityDB.Interfaces;

namespace TrackHub.Security.Infrastructure.SecurityDB.Readers;

public sealed class ServiceClientPermissionReader(IApplicationDbContext context) : IServiceClientPermissionReader
{
    public async Task<IReadOnlyCollection<ServiceClientPermissionVm>> GetServiceClientPermissionsAsync(string? clientId, Guid? accountId, int skip, int take, CancellationToken cancellationToken)
        => await context.ServiceClientPermissions
            .Where(x => (string.IsNullOrEmpty(clientId) || x.ClientId == clientId)
                && (!accountId.HasValue || x.AccountId == accountId))
            .OrderBy(x => x.ClientId).ThenBy(x => x.Resource).ThenBy(x => x.Action).ThenBy(x => x.ServiceClientPermissionId)
            .Skip(Math.Max(0, skip)).Take(Math.Clamp(take <= 0 ? 50 : take, 1, 500))
            .Select(x => new ServiceClientPermissionVm(
                x.ServiceClientPermissionId,
                x.ClientId,
                x.AccountId,
                x.Resource,
                x.Action,
                x.Scope,
                x.Audience,
                x.Active,
                x.EffectiveFrom,
                x.EffectiveTo,
                x.LastModified))
            .ToListAsync(cancellationToken);

    public async Task<bool> HasPermissionAsync(string clientId, string resource, string action, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return await context.ServiceClientPermissions
            .AnyAsync(permission =>
                permission.Active
                && permission.ClientId == clientId
                && permission.Resource == resource
                && permission.Action == action
                && (!permission.EffectiveFrom.HasValue || permission.EffectiveFrom <= now)
                && (!permission.EffectiveTo.HasValue || permission.EffectiveTo >= now),
                cancellationToken);
    }

    public async Task<bool> HasPermissionAsync(string clientId, string resource, string action, Guid? accountId, IReadOnlyCollection<string> scopes, IReadOnlyCollection<string> audiences, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return await context.ServiceClientPermissions
            .AnyAsync(permission =>
                permission.Active
                && permission.ClientId == clientId
                && permission.Resource == resource
                && permission.Action == action
                && (!permission.AccountId.HasValue || permission.AccountId == accountId)
                && scopes.Contains(permission.Scope)
                && audiences.Contains(permission.Audience)
                && (!permission.EffectiveFrom.HasValue || permission.EffectiveFrom <= now)
                && (!permission.EffectiveTo.HasValue || permission.EffectiveTo >= now),
                cancellationToken);
    }
}
