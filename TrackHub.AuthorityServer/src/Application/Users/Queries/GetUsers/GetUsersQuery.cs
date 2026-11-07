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
using Microsoft.Extensions.Logging;

namespace TrackHub.AuthorityServer.Application.Users.Queries.GetUsers;
public readonly record struct GetUsersQuery(string EmailAddress, string Password) : IRequest<UserVm>;

// Handles the GetUsersQuery and returns a UserVm.
// Login lockout mirrors the staff credential model (see AuthenticateStaffQuery): a rolling
// failed-attempt counter that trips a timed lock and resets on a successful login.
public class GetUsersQueryHandler(IUserReader reader, IUserWriter writer, ILogger<GetUsersQueryHandler> logger) : IRequestHandler<GetUsersQuery, UserVm>
{
    private const int MaximumFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    // One key for every credential failure: a distinct message per account state made the form an
    // e-mail-enumeration oracle, and the password grant echoes it verbatim to an anonymous caller.
    private const string CredentialsRejected = "Email or password is incorrect";

    // Verified against on the unknown-email path so the BCrypt cost is paid either way; otherwise
    // the missing hash is a timing oracle that survives unifying the messages.
    private static readonly string DummyHash = "unused-placeholder".HashPassword();

    // Handles the GetUsersQuery and returns a UserVm
    public async Task<UserVm> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var user = await reader.GetUserAsync(new Domain.Records.UserLoginDto(request.EmailAddress, request.Password), cancellationToken);

        if (user == default)
        {
            DummyHash.VerifyHashedPassword(request.Password);
            logger.LogInformation("Login rejected: no user matches the supplied address.");
            throw new AuthenticationException(CredentialsRejected);
        }

        var now = DateTimeOffset.UtcNow;

        // A locked account answers the generic rejection for any password and counts nothing:
        // answering "locked" only to the right password would confirm it, and counting attempts
        // during the lock would let a guesser extend it indefinitely.
        if (user.LockedUntil.HasValue && user.LockedUntil.Value > now)
        {
            DummyHash.VerifyHashedPassword(request.Password);
            logger.LogInformation("Login rejected: {UserId} is locked until {LockedUntil}.", user.UserId, user.LockedUntil);
            throw new AuthenticationException(CredentialsRejected);
        }

        if (!user.Password.VerifyHashedPassword(request.Password))
        {
            await RecordFailureAsync(user, now, cancellationToken);
            logger.LogInformation("Login rejected: wrong password for {UserId}.", user.UserId);
            throw new AuthenticationException(CredentialsRejected);
        }

        // Account state is disclosed only to a caller that proved it knows the password, so a
        // legitimate user still learns why sign-in failed while an attacker learns nothing.
        if (user.Verified == null)
            throw new AuthenticationException("User account hasn't been verified");

        if (!user.Active)
            throw new AuthenticationException("User account is inactive");

        if (user.AccountId == Guid.Empty)
            throw new AuthenticationException("User account is missing tenant assignment");

        await writer.RecordLoginSuccessAsync(user.UserId, cancellationToken);
        user.Password = string.Empty;
        return user;
    }

    // An expired lock resets the counter: it used to survive, so after the first lockout a single
    // wrong password re-locked the account for another window, indefinitely.
    private async Task RecordFailureAsync(UserVm user, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var lockExpired = user.LockedUntil.HasValue && user.LockedUntil.Value <= now;
        var loginAttempts = (lockExpired ? 0 : user.LoginAttempts) + 1;

        DateTimeOffset? lockedUntil = loginAttempts >= MaximumFailedAttempts ? now.Add(LockoutDuration) : null;
        await writer.RecordLoginFailureAsync(user.UserId, loginAttempts, lockedUntil, cancellationToken);
    }
}
