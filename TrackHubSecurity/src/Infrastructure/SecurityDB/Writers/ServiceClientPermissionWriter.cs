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
using TrackHub.Security.Domain.Records;
using TrackHub.Security.Infrastructure.SecurityDB.Interfaces;

namespace TrackHub.Security.Infrastructure.SecurityDB.Writers;

public sealed class ServiceClientPermissionWriter(IApplicationDbContext context) : IServiceClientPermissionWriter
{
    public async Task<ServiceClientPermissionVm> CreateServiceClientPermissionAsync(ServiceClientPermissionDto permission, CancellationToken cancellationToken)
    {
        await GuardAgainstDuplicateAsync(permission, null, cancellationToken);

        var entity = new ServiceClientPermission(
            permission.ClientId,
            permission.AccountId,
            permission.Resource,
            permission.Action,
            permission.Scope,
            permission.Audience,
            permission.Active)
        {
            EffectiveFrom = permission.EffectiveFrom,
            EffectiveTo = permission.EffectiveTo
        };

        await context.ServiceClientPermissions.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task UpdateServiceClientPermissionAsync(Guid serviceClientPermissionId, ServiceClientPermissionDto permission, CancellationToken cancellationToken)
    {
        var entity = await context.ServiceClientPermissions
            .FirstOrDefaultAsync(x => x.ServiceClientPermissionId == serviceClientPermissionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ServiceClientPermission), $"{serviceClientPermissionId}");

        await GuardAgainstDuplicateAsync(permission, serviceClientPermissionId, cancellationToken);

        context.ServiceClientPermissions.Attach(entity);
        entity.ClientId = permission.ClientId;
        entity.AccountId = permission.AccountId;
        entity.Resource = permission.Resource;
        entity.Action = permission.Action;
        entity.Scope = permission.Scope;
        entity.Audience = permission.Audience;
        entity.Active = permission.Active;
        entity.EffectiveFrom = permission.EffectiveFrom;
        entity.EffectiveTo = permission.EffectiveTo;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> DeleteServiceClientPermissionAsync(Guid serviceClientPermissionId, CancellationToken cancellationToken)
    {
        var entity = await context.ServiceClientPermissions
            .FirstOrDefaultAsync(x => x.ServiceClientPermissionId == serviceClientPermissionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ServiceClientPermission), $"{serviceClientPermissionId}");

        context.ServiceClientPermissions.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return serviceClientPermissionId;
    }

    // A grant is unique across the full key (client, account, resource, action, scope, audience);
    // the DB carries a matching unique index. Reject duplicates up front with a clear message.
    private async Task GuardAgainstDuplicateAsync(ServiceClientPermissionDto permission, Guid? excludeId, CancellationToken cancellationToken)
    {
        var duplicate = await context.ServiceClientPermissions
            .AnyAsync(x => (!excludeId.HasValue || x.ServiceClientPermissionId != excludeId.Value)
                && x.ClientId == permission.ClientId
                && x.AccountId == permission.AccountId
                && x.Resource == permission.Resource
                && x.Action == permission.Action
                && x.Scope == permission.Scope
                && x.Audience == permission.Audience,
                cancellationToken);

        if (duplicate)
        {
            throw new ConflictException(
                "A service client permission with the same client, account, resource, action, scope and audience already exists.");
        }
    }

    private static ServiceClientPermissionVm ToVm(ServiceClientPermission x)
        => new(x.ServiceClientPermissionId, x.ClientId, x.AccountId, x.Resource, x.Action, x.Scope, x.Audience, x.Active, x.EffectiveFrom, x.EffectiveTo, x.LastModified);
}
