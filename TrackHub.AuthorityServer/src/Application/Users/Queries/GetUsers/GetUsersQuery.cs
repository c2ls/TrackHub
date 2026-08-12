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

using System.Security.Authentication;
using TrackHub.AuthorityServer.Domain.Interfaces;
using TrackHub.AuthorityServer.Domain.Models;
using Common.Domain.Extensions;

namespace TrackHub.AuthorityServer.Application.Users.Queries.GetUsers;

public readonly record struct GetUsersQuery(string EmailAddress, string Password) : IRequest<UserVm>;

// Handles the GetUsersQuery and returns a UserVm.
// Login lockout mirrors the driver credential model (see AuthenticateDriverQuery): a rolling
// failed-attempt counter that trips a timed lock and resets on a successful login.
public class GetUsersQueryHandler(IUserReader reader, IUserWriter writer) : IRequestHandler<GetUsersQuery, UserVm>
{
    private const int MaximumFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    // Handles the GetUsersQuery and returns a UserVm
    public async Task<UserVm> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var user = await reader.GetUserAsync(new Domain.Records.UserLoginDto(request.EmailAddress, request.Password), cancellationToken);

        if (user == default)
            throw new AuthenticationException("Email is incorrect");

        if (user.Verified == null)
            throw new AuthenticationException("User account hasn't been verified");

        if (!user.Active)
            throw new AuthenticationException("User account is inactive");

        if (user.AccountId == Guid.Empty)
            throw new AuthenticationException("User account is missing tenant assignment");

        // Deny while the account is locked, without revealing whether the password was correct.
        if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTimeOffset.UtcNow)
            throw new AuthenticationException("User account is temporarily locked. Please try again later.");

        if (user.Password.VerifyHashedPassword(request.Password))
        {
            await writer.RecordLoginSuccessAsync(user.UserId, cancellationToken);
            user.Password = string.Empty;
            return user;
        }

        var loginAttempts = user.LoginAttempts + 1;
        DateTimeOffset? lockedUntil = loginAttempts >= MaximumFailedAttempts
            ? DateTimeOffset.UtcNow.Add(LockoutDuration)
            : null;
        await writer.RecordLoginFailureAsync(user.UserId, loginAttempts, lockedUntil, cancellationToken);
        throw new AuthenticationException("Password is incorrect");
    }
}
