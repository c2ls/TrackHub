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

using Common.Application.Interfaces;
using TrackHub.Security.Application.Users.Commands.Update;
using TrackHub.Security.Application.Users.Events;
using TrackHub.Security.Domain.Interfaces;
using TrackHub.Security.Domain.Records;

namespace Application.UnitTests.Users;

[TestFixture]
public class UpdatePasswordTests
{
    private Mock<IUserWriter> _writerMock = null!;
    private Mock<IUserReader> _readerMock = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<IPublisher> _publisherMock = null!;
    private Mock<ICurrentPrincipal> _principalMock = null!;

    [SetUp]
    public void Setup()
    {
        _writerMock = new Mock<IUserWriter>();
        _readerMock = new Mock<IUserReader>();
        _userMock = new Mock<IUser>();
        _publisherMock = new Mock<IPublisher>();
        _principalMock = new Mock<ICurrentPrincipal>();
        _readerMock.Setup(x => x.GetUserNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("subject");
    }

    private UpdatePasswordCommandHandler CreateHandler() =>
        new(_writerMock.Object, _readerMock.Object, _userMock.Object, _publisherMock.Object, _principalMock.Object);

    [Test]
    public async Task Handle_SelfServiceChange_DoesNotPropagateToManagerReplica()
    {
        // A plain user changing their OWN password holds Users/Custom but not Users/Edit;
        // publishing UserUpdated would drive Manager's updateUser (Users/Edit) under their token
        // and be rejected. The self path must not propagate.
        var userId = Guid.NewGuid();
        _userMock.Setup(x => x.Id).Returns(userId.ToString());
        var command = new UpdatePasswordCommand(new UserPasswordDto(userId, "New-Passw0rd!"));

        await CreateHandler().Handle(command, CancellationToken.None);

        _writerMock.Verify(x => x.UpdatePasswordAsync(It.IsAny<UserPasswordDto>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<UserUpdated.Notification>(), It.IsAny<CancellationToken>()), Times.Never);
        _readerMock.Verify(x => x.IsManagerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ManagerResettingSubject_PropagatesActivationToManagerReplica()
    {
        // A manager (holds Users/Edit) resetting a subject's password also activates them;
        // propagate the activation so the vw_users views stop hiding the user.
        var managerId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        _userMock.Setup(x => x.Id).Returns(managerId.ToString());
        _readerMock.Setup(x => x.IsManagerAsync(subjectId, managerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var command = new UpdatePasswordCommand(new UserPasswordDto(subjectId, "New-Passw0rd!"));

        await CreateHandler().Handle(command, CancellationToken.None);

        _writerMock.Verify(x => x.UpdatePasswordAsync(It.IsAny<UserPasswordDto>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(x => x.Publish(
            It.Is<UserUpdated.Notification>(n => n.Id == subjectId && n.User.UserId == subjectId && n.User.Active),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_NonManagerOnAnotherUser_Throws()
    {
        var callerId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        _userMock.Setup(x => x.Id).Returns(callerId.ToString());
        _readerMock.Setup(x => x.IsManagerAsync(subjectId, callerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var command = new UpdatePasswordCommand(new UserPasswordDto(subjectId, "New-Passw0rd!"));

        Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateHandler().Handle(command, CancellationToken.None));
        _writerMock.Verify(x => x.UpdatePasswordAsync(It.IsAny<UserPasswordDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
