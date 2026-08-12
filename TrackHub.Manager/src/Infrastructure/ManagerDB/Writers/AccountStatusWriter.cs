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
using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class AccountStatusWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IAccountStatusWriter
{
    public async Task<(AccountVm Account, AccountStatus PreviousStatus)> ChangeStatusAsync(
        Guid accountId, AccountStatus targetStatus, string? reason, CancellationToken cancellationToken)
    {
        var account = await Context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken)
            ?? throw new NotFoundException(nameof(Account), $"{accountId}");

        Context.Accounts.Attach(account);

        var previous = (AccountStatus)account.Status;
        account.Status = (short)targetStatus;
        account.Active = targetStatus.IsOperational();
        account.StatusChangedAt = DateTimeOffset.UtcNow;

        AddAuditEvent(
            accountId,
            "AccountStatusChanged",
            "Account",
            accountId.ToString(),
            $"{{\"status\":{Quote(previous.ToString())}}}",
            $"{{\"status\":{Quote(targetStatus.ToString())},\"reason\":{Quote(reason)}}}");

        await Context.SaveChangesAsync(cancellationToken);

        var vm = new AccountVm(
            account.AccountId, account.Name, account.Description,
            (AccountType)account.Type, account.Type,
            targetStatus, (short)targetStatus, account.Active, account.LastModified);

        return (vm, previous);
    }
}
