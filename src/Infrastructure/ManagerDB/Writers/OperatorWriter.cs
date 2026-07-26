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

using Common.Application.Interfaces;
using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

// OperatorWriter class responsible for creating, updating, and deleting operators
public sealed class OperatorWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IOperatorWriter
{
    // CreateOperatorAsync method creates a new operator
    public async Task<OperatorVm> CreateOperatorAsync(OperatorDto operatorDto, Guid accountId, CancellationToken cancellationToken)
    {
        var @operator = new Operator(
            operatorDto.Name,
            operatorDto.Description,
            operatorDto.PhoneNumber,
            operatorDto.EmailAddress,
            operatorDto.Address,
            operatorDto.ContactName,
            operatorDto.ProtocolTypeId,
            accountId)
        {
            SyncIntervalMinutes = operatorDto.SyncIntervalMinutes
        };

        await Context.Operators.AddAsync(@operator, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        return new OperatorVm(
            @operator.OperatorId,
            @operator.Name,
            @operator.Description,
            @operator.PhoneNumber,
            @operator.EmailAddress,
            @operator.Address,
            @operator.ContactName,
            (ProtocolType)@operator.ProtocolType,
            @operator.ProtocolType,
            @operator.AccountId,
            @operator.LastModified,
            null,
            @operator.Enabled,
            @operator.SyncIntervalMinutes,
            OperatorHealthStatus.Unknown,
            @operator.LastSuccessfulSyncAt,
            null,
            @operator.LastManualSyncAt,
            @operator.LastDeviceSyncAt,
            null,
            null,
            null,
            null);
    }

    // UpdateOperatorAsync method updates an existing operator
    public async Task UpdateOperatorAsync(UpdateOperatorDto operatorDto, CancellationToken cancellationToken)
    {
        var @operator = await Context.Operators.FindAsync([operatorDto.OperatorId], cancellationToken)
            ?? throw new NotFoundException(nameof(Operator), $"{operatorDto.OperatorId}");
        RequireAccountWriteAccess(@operator.AccountId);

        Context.Operators.Attach(@operator);

        @operator.Name = operatorDto.Name;
        @operator.Description = operatorDto.Description;
        @operator.PhoneNumber = operatorDto.PhoneNumber;
        @operator.EmailAddress = operatorDto.EmailAddress;
        @operator.Address = operatorDto.Address;
        @operator.ContactName = operatorDto.ContactName;
        @operator.ProtocolType = operatorDto.ProtocolTypeId;
        @operator.SyncIntervalMinutes = operatorDto.SyncIntervalMinutes;

        await Context.SaveChangesAsync(cancellationToken);
    }

    // DeleteOperatorAsync method deletes an existing operator
    public async Task DeleteOperatorAsync(Guid operatorId, CancellationToken cancellationToken)
    {
        var @operator = await Context.Operators.FindAsync([operatorId], cancellationToken)
            ?? throw new NotFoundException(nameof(Operator), $"{operatorId}");
        RequireAccountWriteAccess(@operator.AccountId);

        Context.Operators.Attach(@operator);

        Context.Operators.Remove(@operator);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetEnabledAsync(Guid operatorId, bool enabled, CancellationToken cancellationToken)
    {
        var @operator = await Context.Operators.FindAsync([operatorId], cancellationToken)
            ?? throw new NotFoundException(nameof(Operator), $"{operatorId}");
        RequireAccountWriteAccess(@operator.AccountId);

        Context.Operators.Attach(@operator);
        @operator.Enabled = enabled;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkManualSyncTriggeredAsync(Guid operatorId, DateTimeOffset triggeredAt, CancellationToken cancellationToken)
    {
        var @operator = await Context.Operators.FindAsync([operatorId], cancellationToken)
            ?? throw new NotFoundException(nameof(Operator), $"{operatorId}");
        RequireAccountWriteAccess(@operator.AccountId);
        Context.Operators.Attach(@operator);
        @operator.LastManualSyncAt = triggeredAt;
        await Context.SaveChangesAsync(cancellationToken);
    }

    // Stamps the successful device-sync timestamps the schedulers gate on. Failure detail,
    // health, and position-sync history are telemetry-derived at read time (sync runs +
    // health checks), not row state.
    public async Task UpdateSyncSummaryAsync(Guid operatorId, DateTimeOffset finishedAt, SyncTriggerType trigger, CancellationToken cancellationToken)
    {
        var @operator = await Context.Operators.FindAsync([operatorId], cancellationToken)
            ?? throw new NotFoundException(nameof(Operator), $"{operatorId}");
        RequireAccountWriteAccess(@operator.AccountId);
        Context.Operators.Attach(@operator);

        @operator.LastSuccessfulSyncAt = finishedAt;
        @operator.LastDeviceSyncAt = finishedAt;
        if (trigger == SyncTriggerType.Manual)
        {
            @operator.LastManualSyncAt = finishedAt;
        }

        await Context.SaveChangesAsync(cancellationToken);
    }
}
