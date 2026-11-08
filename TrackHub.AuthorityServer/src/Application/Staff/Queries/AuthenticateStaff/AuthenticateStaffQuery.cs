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

using System.Security.Authentication;
using Common.Domain.Extensions;
using TrackHub.AuthorityServer.Domain.Interfaces;

namespace TrackHub.AuthorityServer.Application.Staff.Queries.AuthenticateStaff;

public readonly record struct AuthenticateStaffQuery(string Login, string Password) : IRequest<AuthenticatedStaffMemberVm>;

/// <summary>
/// Carries the credential that authenticated, not just the staff: a staff may hold several
/// credentials, and revoking one has to invalidate the sessions issued from THAT one.
/// </summary>
public readonly record struct AuthenticatedStaffMemberVm(Guid StaffId, Guid AccountId, Guid StaffCredentialId, Guid SecurityStamp);

public sealed class AuthenticateStaffQueryHandler(IStaffCredentialReader reader, IStaffCredentialWriter writer) : IRequestHandler<AuthenticateStaffQuery, AuthenticatedStaffMemberVm>
{
    private const int MaximumFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    // One key for every credential failure, and account state only after the password is proven:
    // see GetUsersQueryHandler for the enumeration and timing oracles this closes.
    public const string CredentialsRejected = "Staff credential is incorrect";
    private static readonly string DummyHash = "unused-placeholder".HashPassword();

    public async Task<AuthenticatedStaffMemberVm> Handle(AuthenticateStaffQuery request, CancellationToken cancellationToken)
    {
        var normalizedLogin = request.Login.Trim().ToUpperInvariant();
        var credential = await reader.GetStaffCredentialByLoginAsync(normalizedLogin, cancellationToken);

        if (credential == null || !credential.Active)
        {
            DummyHash.VerifyHashedPassword(request.Password);
            throw new AuthenticationException(CredentialsRejected);
        }

        var now = DateTimeOffset.UtcNow;
        var lockExpired = credential.LockedUntil.HasValue && credential.LockedUntil.Value <= now;

        // A locked credential answers the generic rejection for any password and counts nothing.
        if (credential.LockedUntil.HasValue && credential.LockedUntil.Value > now)
        {
            DummyHash.VerifyHashedPassword(request.Password);
            throw new AuthenticationException(CredentialsRejected);
        }

        if (!credential.PasswordHash.VerifyHashedPassword(request.Password))
        {
            // An expired lock resets the counter: it used to survive, so after the first lockout a
            // single wrong password re-locked the credential for another window, indefinitely.
            var failedAttempts = (lockExpired ? 0 : credential.FailedAttempts) + 1;
            DateTimeOffset? lockedUntil = failedAttempts >= MaximumFailedAttempts ? now.Add(LockoutDuration) : null;
            await writer.RecordStaffCredentialLoginFailureAsync(credential.StaffCredentialId, failedAttempts, lockedUntil, cancellationToken);
            throw new AuthenticationException(CredentialsRejected);
        }

        if (credential.VerifiedAt == null || credential.ResetRequired)
        {
            throw new AuthenticationException("Staff credential activation is required");
        }

        await writer.RecordStaffCredentialLoginSuccessAsync(credential.StaffCredentialId, cancellationToken);
        return new AuthenticatedStaffMemberVm(credential.StaffId, credential.AccountId, credential.StaffCredentialId, credential.SecurityStamp);
    }
}
