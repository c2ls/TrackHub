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

using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

// AccountWriter class is responsible for writing account-related data to the database
public sealed class AccountWriter(IApplicationDbContext context) : IAccountWriter
{
    // Creates a new account asynchronously
    // Parameters:
    // - accountDto: The account data transfer object
    // - cancellationToken: The cancellation token
    // Returns:
    // - The created account view model
    public async Task<AccountVm> CreateAccountAsync(AccountDto accountDto, CancellationToken cancellationToken)
    {
        var account = new Account(
            accountDto.Name,
            accountDto.Description,
            accountDto.TypeId,
            accountDto.Active);

        await context.Accounts.AddAsync(account, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new AccountVm(
            account.AccountId,
            account.Name,
            account.Description,
            (AccountType)account.Type,
            account.Type,
            (AccountStatus)account.Status,
            account.Status,
            account.Active,
            account.LastModified);
    }

    // Updates an existing account asynchronously
    // Parameters:
    // - accountDto: The updated account data transfer object
    // - cancellationToken: The cancellation token
    public async Task UpdateAccountAsync(UpdateAccountDto accountDto, CancellationToken cancellationToken)
    {
        var account = await context.Accounts.FindAsync(accountDto.AccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(Account), $"{accountDto.AccountId}");

        context.Accounts.Attach(account);

        account.Name = accountDto.Name;
        account.Description = accountDto.Description;
        account.Type = accountDto.TypeId;

        // The legacy `active` flag can no longer change lifecycle state:
        // suspend/cancel/archive and re-activation all require ChangeAccountStatus. Keep Active as the
        // derived read of the unchanged Status so updateAccount can never move an account out of, or
        // into, the operational band {Trial, Active}.
        account.Active = ((AccountStatus)account.Status).IsOperational();

        await context.SaveChangesAsync(cancellationToken);
    }
}
