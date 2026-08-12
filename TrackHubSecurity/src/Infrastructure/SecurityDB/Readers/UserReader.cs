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

using Common.Domain.Helpers;
using TrackHub.Security.Infrastructure.SecurityDB.Interfaces;

namespace TrackHub.Security.Infrastructure.SecurityDB.Readers;

/// <summary>
/// This class represents a reader for retrieving user information from the database
/// </summary>
/// <param name="context"></param>
public sealed class UserReader(IApplicationDbContext context) : IUserReader
{
    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    /// <summary>
    /// Retrieves the username of a user with the specified ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The username of the user with the specified ID</returns>
    public async Task<string> GetUserNameAsync(Guid id, CancellationToken cancellationToken)
        => await context.Users
            .Where(u => u.UserId.Equals(id))
            .Select(u => u.Username)
            .FirstAsync(cancellationToken);

    /// <summary>
    /// Retrieves a collection of users based on the specified filters
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A collection of user view models</returns>
    public async Task<IReadOnlyCollection<UserVm>> GetUsersAsync(Filters filters, int skip, int take, CancellationToken cancellationToken)
    {
        var query = context.Users.AsQueryable();
        query = filters.Apply(query);

        return await query
            .OrderBy(u => u.Username).ThenBy(u => u.UserId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(u => new UserVm(
                u.UserId,
                u.Username,
                u.EmailAddress,
                u.FirstName,
                u.SecondName,
                u.LastName,
                u.SecondSurname,
                u.DOB,
                u.LoginAttempts,
                u.LockedUntil,
                u.AccountId,
                u.Active,
                u.IntegrationUser,
                null,
                null))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves the detailed information of a user with the specified ID, including their roles and policies
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A user view model</returns>
    public async Task<UserVm> GetUserAsync(Guid id, CancellationToken cancellationToken)
        => await context.Users
            .Where(u => u.UserId.Equals(id))
            .Include(role => role.Roles)
            .Include(policy => policy.Policies)
            .Select(u => new UserVm(
                u.UserId,
                u.Username,
                u.EmailAddress,
                u.FirstName,
                u.SecondName,
                u.LastName,
                u.SecondSurname,
                u.DOB,
                u.LoginAttempts,
                u.LockedUntil,
                u.AccountId,
                u.Active,
                u.IntegrationUser,
                u.Roles.Select(r => new RoleVm(r.RoleId, r.Name)).ToList(),
                u.Policies.Select(p => new PolicyVm(p.PolicyId, p.Name)).ToList()))
            .FirstAsync(cancellationToken);

    /// <summary>
    /// Retrieves a collection of users associated with the specified account ID, including their roles and policies
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A collection of user view models</returns>
    public async Task<IReadOnlyCollection<UserVm>> GetUsersAsync(Guid accountId, int skip, int take, CancellationToken cancellationToken)
        => await context.Users
            .Where(u => u.AccountId.Equals(accountId))
            .Include(role => role.Roles)
            .Include(policy => policy.Policies)
            .OrderBy(u => u.Username).ThenBy(u => u.UserId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(u => new UserVm(
                u.UserId,
                u.Username,
                u.EmailAddress,
                u.FirstName,
                u.SecondName,
                u.LastName,
                u.SecondSurname,
                u.DOB,
                u.LoginAttempts,
                u.LockedUntil,
                u.AccountId,
                u.Active,
                u.IntegrationUser,
                u.Roles.Select(r => new RoleVm(r.RoleId, r.Name)).ToList(),
                u.Policies.Select(p => new PolicyVm(p.PolicyId, p.Name)).ToList()))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Retrieves a collection of users associated with the specified account ID and role ID
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="roleId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A collection of user view models</returns>
    public async Task<IReadOnlyCollection<UserVm>> GetUsersByRoleAsync(Guid accountId, int roleId, CancellationToken cancellationToken)
    {
        return await context.Users
            .Where(u => u.AccountId == accountId && u.Roles.Any(r => r.RoleId == roleId))
            .Include(u => u.Roles)
            .Select(u => new UserVm(
                u.UserId,
                u.Username,
                u.EmailAddress,
                u.FirstName,
                u.SecondName,
                u.LastName,
                u.SecondSurname,
                u.DOB,
                u.LoginAttempts,
                u.LockedUntil,
                u.AccountId,
                u.Active,
                u.IntegrationUser,
                null,
                null))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a collection of users associated with the specified account ID and policy ID
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="policyId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A collection of user view models</returns>
    public async Task<IReadOnlyCollection<UserVm>> GetUsersByPolicyAsync(Guid accountId, int policyId, CancellationToken cancellationToken)
    {
        return await context.Users
            .Where(u => u.AccountId == accountId && u.Policies.Any(r => r.PolicyId == policyId))
            .Include(u => u.Policies)
            .Select(u => new UserVm(
                u.UserId,
                u.Username,
                u.EmailAddress,
                u.FirstName,
                u.SecondName,
                u.LastName,
                u.SecondSurname,
                u.DOB,
                u.LoginAttempts,
                u.LockedUntil,
                u.AccountId,
                u.Active,
                u.IntegrationUser,
                null,
                null))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Validates if the specified email address is unique 
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A boolean indicating whether the specified email address is unique</returns>
    public async Task<bool> ValidateEmailAddressAsync(string emailAddress, CancellationToken cancellationToken)
        => !await context.Users
            .Where(u => u.EmailAddress.Equals(emailAddress))
            .AnyAsync(cancellationToken);

    /// <summary>
    /// Validates if the specified email address is unique for a user other than the one with the specified ID 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="emailAddress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A boolean indicating whether the specified email address is unique</returns>
    public async Task<bool> ValidateEmailAddressAsync(Guid userId, string emailAddress, CancellationToken cancellationToken)
        => !await context.Users
            .Where(u => !u.UserId.Equals(userId) && u.EmailAddress.Equals(emailAddress))
            .AnyAsync(cancellationToken);

    /// <summary>
    /// Validates if the specified username is unique for a user other than the one with the specified ID
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="username"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A boolean indicating whether the specified username is unique</returns>
    public async Task<bool> ValidateUsernameAsync(Guid userId, string username, CancellationToken cancellationToken)
        => !await context.Users
            .Where(u => !u.UserId.Equals(userId) && u.Username.Equals(username))
            .AnyAsync(cancellationToken);

    /// <summary>
    /// Validates whether the specified user is an administrator 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A boolean indicating whether the specified user is an administrator</returns>
    public async Task<bool> IsAdminAsync(Guid userId, CancellationToken cancellationToken)
        => await context.Users
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.Roles)
            .AnyAsync(r => r.ParentRoleId == null, cancellationToken);

    /// <summary>
    /// Validates whether the specified user is a manager
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A boolean indicating whether the specified user is a manager</returns>
    public async Task<bool> IsManagerAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Users
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.Roles)
            .AnyAsync(r => context.Roles
                .Where(pr => pr.RoleId == r.ParentRoleId && pr.ParentRoleId == null)
                .Any(), cancellationToken);
    }

    /// <summary>
    /// Validates whether a user is a manager of the specified user 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="managerId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A boolean indicating whether the specified user is a manager</returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<bool> IsManagerAsync(Guid userId, Guid managerId, CancellationToken cancellationToken)
    {
        // Retrieve the account ID of the user
        var user = await context.Users.FindAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), $"{userId}");

        // Check if the manager is part of the same account and if a role parent of the manager is top level
        return await context.Users
            .Where(u => u.UserId == managerId && u.AccountId == user.AccountId)
            .SelectMany(u => u.Roles)
            .SelectMany(r => context.Roles
                .Where(pr => pr.RoleId == r.ParentRoleId && pr.ParentRoleId == null))
            .AnyAsync(cancellationToken);
    }

}
