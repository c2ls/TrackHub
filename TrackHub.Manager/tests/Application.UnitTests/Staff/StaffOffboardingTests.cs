using Common.Domain.Constants;
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

using TrackHub.Manager.Application.Staff.Commands;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Domain.Records;

namespace TrackHub.Manager.Application.UnitTests.Staff;

// Deactivating a staff must end their sign-in credential in Security, and must do so BEFORE the
// local write so a Security outage leaves the staff active rather than deactivated-but-signing-in.
[TestFixture]
public class StaffOffboardingTests
{
    private static readonly Guid StaffId = Guid.NewGuid();
    private static readonly Guid AccountId = Guid.NewGuid();

    private Mock<IStaffMemberReader> _reader = null!;
    private Mock<IStaffCredentialRevoker> _revoker = null!;
    private Mock<IStaffMemberWriter> _writer = null!;
    private readonly List<string> _calls = [];

    [SetUp]
    public void SetUp()
    {
        _calls.Clear();
        _reader = new Mock<IStaffMemberReader>();
        _revoker = new Mock<IStaffCredentialRevoker>();
        _writer = new Mock<IStaffMemberWriter>();
        _revoker.Setup(r => r.RevokeStaffCredentialsAsync(StaffId, AccountId, It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("revoke")).Returns(Task.CompletedTask);
        _writer.Setup(w => w.DeactivateStaffMemberAsync(StaffId, It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("write")).Returns(Task.CompletedTask);
        _writer.Setup(w => w.UpdateStaffMemberAsync(StaffId, It.IsAny<StaffMemberDto>(), It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("write")).Returns(Task.CompletedTask);
    }

    private void CurrentStaff(bool active)
        => _reader.Setup(r => r.GetStaffMemberAsync(StaffId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StaffMemberVm(StaffId, AccountId, "Staff", null, null, null, active, null, null, null, null, StaffTypes.Driver, DateTimeOffset.UtcNow, 0));

    private static StaffMemberDto Dto(bool active) => new() { AccountId = AccountId, Name = "Staff", Active = active };

    [Test]
    public async Task Deactivate_RevokesCredentialsBeforeTheLocalWrite()
    {
        CurrentStaff(active: true);

        await new DeactivateStaffMemberCommandHandler(_reader.Object, _revoker.Object, _writer.Object)
            .Handle(new DeactivateStaffMemberCommand(StaffId), CancellationToken.None);

        Assert.That(_calls, Is.EqualTo(new[] { "revoke", "write" }));
    }

    [Test]
    public async Task Deactivate_WhenSecurityRefuses_LeavesTheStaffActive()
    {
        CurrentStaff(active: true);
        _revoker.Setup(r => r.RevokeStaffCredentialsAsync(StaffId, AccountId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("security down"));

        await Assert.ThrowsAsync<HttpRequestException>(() => new DeactivateStaffMemberCommandHandler(_reader.Object, _revoker.Object, _writer.Object)
            .Handle(new DeactivateStaffMemberCommand(StaffId), CancellationToken.None));

        _writer.Verify(w => w.DeactivateStaffMemberAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Update_ThatDeactivates_RevokesCredentialsFirst()
    {
        CurrentStaff(active: true);

        await new UpdateStaffMemberCommandHandler(_reader.Object, _revoker.Object, _writer.Object)
            .Handle(new UpdateStaffMemberCommand(StaffId, Dto(active: false)), CancellationToken.None);

        Assert.That(_calls, Is.EqualTo(new[] { "revoke", "write" }));
    }

    [Test]
    public async Task Update_ThatKeepsTheStaffActive_DoesNotTouchSecurity()
    {
        CurrentStaff(active: true);

        await new UpdateStaffMemberCommandHandler(_reader.Object, _revoker.Object, _writer.Object)
            .Handle(new UpdateStaffMemberCommand(StaffId, Dto(active: true)), CancellationToken.None);

        _revoker.VerifyNoOtherCalls();
        Assert.That(_calls, Is.EqualTo(new[] { "write" }));
    }

    [Test]
    public async Task Update_OfAnAlreadyInactiveStaff_DoesNotTouchSecurity()
    {
        CurrentStaff(active: false);

        await new UpdateStaffMemberCommandHandler(_reader.Object, _revoker.Object, _writer.Object)
            .Handle(new UpdateStaffMemberCommand(StaffId, Dto(active: false)), CancellationToken.None);

        _revoker.VerifyNoOtherCalls();
    }
}
