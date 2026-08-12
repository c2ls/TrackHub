// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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
using Common.Domain.Extensions;
using TrackHub.Security.Domain.Records;
using TrackHub.Security.Infrastructure.Interfaces;

namespace TrackHub.Security.Infrastructure.Writers;

// This class represents a writer for the User entity in the security database. Every by-id
// mutation loads the row first and checks its owning account against the caller
// (RequireAccountAccess) — the enforcement point the [AccountScopeEnforcedInHandler] markers on
// the user commands cite.
public sealed class UserWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IUserWriter
{
    /// <summary>
    /// Creates a new user asynchronously
    /// </summary>
    /// <param name="userDto"></param>
    /// <param name="accountId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The created user view model</returns>
    public async Task<UserVm> CreateUserAsync(CreateUserDto userDto, Guid accountId, CancellationToken cancellationToken)
    {
        RequireAccountAccess(accountId);

        // Hash the password
        var password = userDto.Password.HashPassword();

        var user = new User(
            userDto.Username,
            password,
            userDto.EmailAddress,
            userDto.FirstName,
            userDto.SecondName,
            userDto.LastName,
            userDto.SecondSurname,
            userDto.DOB,
            userDto.Active,
            0,
            accountId,
            userDto.IntegrationUser);

        await Context.Users.AddAsync(user, cancellationToken);

        await Context.SaveChangesAsync(cancellationToken);

        return new UserVm(
            user.UserId,
            user.Username,
            user.EmailAddress,
            user.FirstName,
            user.SecondName,
            user.LastName,
            user.SecondSurname,
            user.DOB,
            user.LoginAttempts,
            user.LockedUntil,
            user.AccountId,
            user.Active,
            user.IntegrationUser,
            [],
            []);
    }

    /// <summary>
    /// Updates an existing user asynchronously.
    /// </summary>
    /// <param name="userDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The task result.</returns>
    /// <exception cref="NotFoundException">If the user does not exist</exception>
    public async Task UpdateUserAsync(UpdateUserDto userDto, CancellationToken cancellationToken)
    {
        var user = await Context.Users.FindAsync([userDto.UserId], cancellationToken)
            ?? throw new NotFoundException(nameof(User), $"{userDto.UserId}");
        RequireAccountAccess(user.AccountId);

        Context.Users.Attach(user);

        user.Username = userDto.Username;
        user.EmailAddress = userDto.EmailAddress;
        user.FirstName = userDto.FirstName;
        user.SecondName = userDto.SecondName;
        user.LastName = userDto.LastName;
        user.SecondSurname = userDto.SecondSurname;
        user.DOB = userDto.DOB;
        user.Active = userDto.Active;
        user.IntegrationUser = userDto.IntegrationUser;
        user.LoginAttempts = 0;

        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// This method is used to update the user's information
    /// </summary>
    /// <param name="userDto"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The task result</returns>
    /// <exception cref="NotFoundException">If the user does not exist</exception>
    public async Task UpdateUserAsync(UpdateCurrentUserDto userDto, Guid userId, CancellationToken cancellationToken)
    {
        var user = await Context.Users.FindAsync([userId], cancellationToken)
            ?? throw new NotFoundException(nameof(User), $"{userId}");
        RequireAccountAccess(user.AccountId);

        Context.Users.Attach(user);

        user.FirstName = userDto.FirstName;
        user.SecondName = userDto.SecondName;
        user.LastName = userDto.LastName;
        user.SecondSurname = userDto.SecondSurname;
        user.DOB = userDto.DOB;

        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the password of an existing user asynchronously
    /// </summary>
    /// <param name="userPasswordDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The task result</returns>
    /// <exception cref="NotFoundException">If the user does not exist</exception>
    public async Task UpdatePasswordAsync(UserPasswordDto userPasswordDto, CancellationToken cancellationToken)
    {
        var user = await Context.Users.FindAsync([userPasswordDto.UserId], cancellationToken)
            ?? throw new NotFoundException(nameof(User), $"{userPasswordDto.UserId}");
        RequireAccountAccess(user.AccountId);

        Context.Users.Attach(user);

        var password = userPasswordDto.Password.HashPassword();

        user.Password = password;
        user.Active = true;

        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Clears an account lockout: resets the rolling failed-attempt counter and any timed lock so the
    /// user can sign in immediately. Mirrors the reset the AuthorityServer performs on a successful login.
    /// </summary>
    public async Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await Context.Users.FindAsync([userId], cancellationToken)
            ?? throw new NotFoundException(nameof(User), $"{userId}");
        RequireAccountAccess(user.AccountId);

        Context.Users.Attach(user);
        user.LoginAttempts = 0;
        user.LockedUntil = null;

        await Context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes a user asynchronously
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The task result</returns>
    /// <exception cref="NotFoundException">If the user does not exist</exception>
    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await Context.Users.FindAsync([userId], cancellationToken)
            ?? throw new NotFoundException(nameof(User), $"{userId}");
        RequireAccountAccess(user.AccountId);

        Context.Users.Attach(user);
        Context.Users.Remove(user);

        await Context.SaveChangesAsync(cancellationToken);
    }
}
