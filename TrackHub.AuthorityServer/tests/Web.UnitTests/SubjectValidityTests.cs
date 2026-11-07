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

using System.Security.Claims;
using Moq;
using TrackHub.AuthorityServer.Domain.Interfaces;
using TrackHub.AuthorityServer.Domain.Models;
using TrackHub.AuthorityServer.Web.Helpers;

namespace Web.UnitTests;

// A session is tied to the credential it was issued from and to that credential's security stamp:
// Security re-stamps on deactivation, password change, lock, reset and revocation, so a session
// minted before any of those ends at the next authorize or refresh.
[TestFixture]
public class SubjectValidityTests
{
    private static readonly Guid StaffId = Guid.NewGuid();
    private static readonly Guid CredentialId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid Stamp = Guid.NewGuid();

    private Mock<IStaffCredentialReader> _credentials = null!;
    private Mock<IUserReader> _users = null!;
    private SubjectValidity _validity = null!;

    [SetUp]
    public void SetUp()
    {
        _credentials = new Mock<IStaffCredentialReader>();
        _users = new Mock<IUserReader>();
        _validity = new SubjectValidity(_users.Object, _credentials.Object);
    }

    private static ClaimsPrincipal Principal(params Claim[] claims)
        => new(new ClaimsIdentity(claims, "test"));

    private static ClaimsPrincipal StaffPrincipal(Guid? stamp = null)
        => Principal(
            new Claim("principal_type", "Staff"),
            new Claim("staff_id", StaffId.ToString()),
            new Claim("staff_credential_id", CredentialId.ToString()),
            new Claim(SubjectValidity.SecurityStampClaim, (stamp ?? Stamp).ToString()));

    private static ClaimsPrincipal UserPrincipal(Guid? stamp = null)
        => Principal(
            new Claim("principal_type", "User"),
            new Claim("user_id", UserId.ToString()),
            new Claim(SubjectValidity.SecurityStampClaim, (stamp ?? Stamp).ToString()));

    private void Credential(bool active = true, DateTimeOffset? lockedUntil = null, bool resetRequired = false, Guid? stamp = null)
        => _credentials.Setup(r => r.GetCredentialSessionAsync(CredentialId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StaffCredentialSessionVm(active, lockedUntil, resetRequired, stamp ?? Stamp));

    private void User(bool active = true, DateTimeOffset? lockedUntil = null, Guid? stamp = null)
        => _users.Setup(r => r.GetUserAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserVm(UserId, "user", string.Empty, "user@mail.com", DateTimeOffset.UtcNow, active, 0, lockedUntil, Guid.NewGuid(), stamp ?? Stamp));

    [Test]
    public async Task ALiveStaffCredentialWithTheSameStamp_KeepsItsSession()
    {
        Credential();
        Assert.That(await _validity.IsStillValidAsync(StaffPrincipal(), CancellationToken.None), Is.True);
    }

    [Test]
    public async Task ARevokedCredentialEndsItsOwnSession()
    {
        Credential(active: false);
        Assert.That(await _validity.IsStillValidAsync(StaffPrincipal(), CancellationToken.None), Is.False);
    }

    [Test]
    public async Task ALockedCredentialEndsItsSession()
    {
        Credential(lockedUntil: DateTimeOffset.UtcNow.AddMinutes(10));
        Assert.That(await _validity.IsStillValidAsync(StaffPrincipal(), CancellationToken.None), Is.False);
    }

    [Test]
    public async Task ACredentialAwaitingResetEndsItsSession()
    {
        Credential(resetRequired: true);
        Assert.That(await _validity.IsStillValidAsync(StaffPrincipal(), CancellationToken.None), Is.False);
    }

    [Test]
    public async Task AReStampedCredentialEndsTheSessionsMintedBefore()
    {
        Credential(stamp: Guid.NewGuid());
        Assert.That(await _validity.IsStillValidAsync(StaffPrincipal(), CancellationToken.None), Is.False);
    }

    [Test]
    public async Task AStaffSessionWithoutACredentialOrStampClaim_IsEnded()
    {
        Credential();
        var noStamp = Principal(new Claim("principal_type", "Staff"), new Claim("staff_credential_id", CredentialId.ToString()));
        var noCredential = Principal(new Claim("principal_type", "Staff"), new Claim(SubjectValidity.SecurityStampClaim, Stamp.ToString()));

        Assert.Multiple(async () =>
        {
            Assert.That(await _validity.IsStillValidAsync(noStamp, CancellationToken.None), Is.False);
            Assert.That(await _validity.IsStillValidAsync(noCredential, CancellationToken.None), Is.False);
        });
    }

    [Test]
    public async Task AnActiveUserWithTheSameStamp_KeepsTheSession()
    {
        User();
        Assert.That(await _validity.IsStillValidAsync(UserPrincipal(), CancellationToken.None), Is.True);
    }

    [Test]
    public async Task ADeactivatedOrLockedUser_LosesTheSession()
    {
        User(active: false);
        Assert.That(await _validity.IsStillValidAsync(UserPrincipal(), CancellationToken.None), Is.False);

        User(lockedUntil: DateTimeOffset.UtcNow.AddMinutes(5));
        Assert.That(await _validity.IsStillValidAsync(UserPrincipal(), CancellationToken.None), Is.False);
    }

    [Test]
    public async Task APasswordChangeReStampsTheUser_AndEndsOlderSessions()
    {
        User(stamp: Guid.NewGuid());
        Assert.That(await _validity.IsStillValidAsync(UserPrincipal(), CancellationToken.None), Is.False);
    }

    [Test]
    public async Task AUserSessionWithoutAStampClaim_IsEnded()
    {
        User();
        var noStamp = Principal(new Claim("principal_type", "User"), new Claim("user_id", UserId.ToString()));
        Assert.That(await _validity.IsStillValidAsync(noStamp, CancellationToken.None), Is.False);
    }

    [Test]
    public async Task AServiceClientSessionIsNeverReValidated()
    {
        var service = Principal(new Claim("principal_type", "ServiceClient"));
        Assert.That(await _validity.IsStillValidAsync(service, CancellationToken.None), Is.True);
        _users.VerifyNoOtherCalls();
        _credentials.VerifyNoOtherCalls();
    }
}
