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

using TrackHub.Manager.Application.Accounts.Events;

namespace TrackHub.Manager.Application.Accounts.Commands.Create;

[Authorize(Resource = Resources.Administrative, Action = Actions.Write)]
public readonly record struct CreateAccountCommand(AccountDto Account) : IRequest<AccountVm>;

public class CreateAccountCommandHandler(IAccountWriter writer, ISecurityWriter securityWriter, IPublisher publisher) : IRequestHandler<CreateAccountCommand, AccountVm>
{
    // This class handles the logic for creating an account
    public async Task<AccountVm> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    { 
        var account = await writer.CreateAccountAsync(request.Account, cancellationToken);
        await publisher.Publish(new AccountCreated.Notification(account.AccountId), cancellationToken);
        await securityWriter.CreateUserAsync(new CreateUserDto
        (
            account.AccountId,
            Roles.Manager,
            request.Account.Password,
            request.Account.EmailAddress,
            request.Account.FirstName,
            request.Account.LastName,
            request.Account.Active
        ), cancellationToken);

        return account;
    }
}
