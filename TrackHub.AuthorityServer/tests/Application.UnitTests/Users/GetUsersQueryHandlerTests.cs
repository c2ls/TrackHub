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
using TrackHub.AuthorityServer.Application.Users.Queries.GetUsers;
using TrackHub.AuthorityServer.Domain.Interfaces;
using TrackHub.AuthorityServer.Domain.Models;
using TrackHub.AuthorityServer.Domain.Records;

namespace TrackHub.AuthorityServer.Application.UnitTests.Users;

// User login lockout mirrors the driver model — a rolling failed-attempt counter
// that trips a 15-minute timed lock on the 5th failure and resets on success. A locked account is
// denied before the password is checked (no lockout-vs-wrong-password enumeration, no counter change).
[TestFixture]
public class GetUsersQueryHandlerTests
{
    private const string CorrectPassword = "Correct-Horse-1";
    private const string WrongPassword = "wrong-password";

    private Mock<IUserReader> _reader = null!;
    private Mock<IUserWriter> _writer = null!;

    [SetUp]
    public void SetUp()
    {
        _reader = new Mock<IUserReader>();
        _writer = new Mock<IUserWriter>();
    }

    private static UserVm ActiveUser(int loginAttempts = 0, DateTimeOffset? lockedUntil = null)
        => new(
            UserId: Guid.NewGuid(),
            Username: "user",
            Password: CorrectPassword.HashPassword(),
            EmailAddress: "user@mail.com",
            Verified: DateTimeOffset.UtcNow,
            Active: true,
            LoginAttempts: loginAttempts,
            LockedUntil: lockedUntil,
            AccountId: Guid.NewGuid());

    private GetUsersQueryHandler Handler() => new(_reader.Object, _writer.Object);

    private void SetupReader(UserVm user)
        => _reader.Setup(r => r.GetUserAsync(It.IsAny<UserLoginDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

    [Test]
    public async Task CorrectPassword_RecordsSuccess_AndReturnsUserWithoutPassword()
    {
        var user = ActiveUser(loginAttempts: 3);
        SetupReader(user);

        var result = await Handler().Handle(new GetUsersQuery(user.EmailAddress, CorrectPassword), CancellationToken.None);

        Assert.That(result.UserId, Is.EqualTo(user.UserId));
        Assert.That(result.Password, Is.Empty, "the password hash must be cleared from the returned view model");
        _writer.Verify(w => w.RecordLoginSuccessAsync(user.UserId, It.IsAny<CancellationToken>()), Times.Once);
        _writer.Verify(w => w.RecordLoginFailureAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void WrongPassword_BelowThreshold_RecordsIncrementedAttempt_WithoutLock()
    {
        var user = ActiveUser(loginAttempts: 2);
        SetupReader(user);

        Assert.ThrowsAsync<AuthenticationException>(async () =>
            await Handler().Handle(new GetUsersQuery(user.EmailAddress, WrongPassword), CancellationToken.None));

        // 3rd failure — below the 5-attempt threshold, so no lock is set.
        _writer.Verify(w => w.RecordLoginFailureAsync(user.UserId, 3, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void WrongPassword_FifthAttempt_SetsTimedLock()
    {
        var user = ActiveUser(loginAttempts: 4);
        SetupReader(user);

        Assert.ThrowsAsync<AuthenticationException>(async () =>
            await Handler().Handle(new GetUsersQuery(user.EmailAddress, WrongPassword), CancellationToken.None));

        // The 5th consecutive failure trips the lock (~15 minutes ahead).
        _writer.Verify(w => w.RecordLoginFailureAsync(
            user.UserId,
            5,
            It.Is<DateTimeOffset?>(d => d.HasValue && d.Value > DateTimeOffset.UtcNow.AddMinutes(10)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void LockedAccount_DeniesBeforePasswordCheck_WithoutTouchingCounter()
    {
        // Even with the CORRECT password, a currently-locked account is denied and the counter is untouched.
        var user = ActiveUser(loginAttempts: 5, lockedUntil: DateTimeOffset.UtcNow.AddMinutes(10));
        SetupReader(user);

        var ex = Assert.ThrowsAsync<AuthenticationException>(async () =>
            await Handler().Handle(new GetUsersQuery(user.EmailAddress, CorrectPassword), CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("locked").IgnoreCase);
        Assert.That(ex.Message, Does.Not.Contain("Password").IgnoreCase, "the lock message must not reveal password correctness");
        _writer.Verify(w => w.RecordLoginSuccessAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _writer.Verify(w => w.RecordLoginFailureAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<DateTimeOffset?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExpiredLock_AllowsLoginAgain_AndResetsOnSuccess()
    {
        // A lock in the past no longer blocks — the correct password succeeds and resets the counter.
        var user = ActiveUser(loginAttempts: 5, lockedUntil: DateTimeOffset.UtcNow.AddMinutes(-1));
        SetupReader(user);

        var result = await Handler().Handle(new GetUsersQuery(user.EmailAddress, CorrectPassword), CancellationToken.None);

        Assert.That(result.UserId, Is.EqualTo(user.UserId));
        _writer.Verify(w => w.RecordLoginSuccessAsync(user.UserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void UnknownEmail_ThrowsAuthentication()
    {
        _reader.Setup(r => r.GetUserAsync(It.IsAny<UserLoginDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(default(UserVm));

        Assert.ThrowsAsync<AuthenticationException>(async () =>
            await Handler().Handle(new GetUsersQuery("nobody@mail.com", CorrectPassword), CancellationToken.None));
        _writer.VerifyNoOtherCalls();
    }

    [Test]
    public void InactiveUser_ThrowsAuthentication_WithoutRecordingFailure()
    {
        var user = ActiveUser() with { Active = false };
        SetupReader(user);

        Assert.ThrowsAsync<AuthenticationException>(async () =>
            await Handler().Handle(new GetUsersQuery(user.EmailAddress, CorrectPassword), CancellationToken.None));
        _writer.VerifyNoOtherCalls();
    }
}
