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
using TrackHub.Security.Application.ResourceActionRole.Commands.Create;

namespace Application.UnitTests.ResourceActionRole;

[TestFixture]
public class CreateResourceActionRoleCommandHandlerTests
{
    private Mock<IResourceActionRoleWriter> _writerMock;
    private Mock<IUserReader> _userReaderMock;
    private Mock<IUser> _userMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<IResourceActionRoleWriter>();
        _userReaderMock = new Mock<IUserReader>();
        _userMock = new Mock<IUser>();
    }

    [Test]
    public async Task Handle_AdminUser_CreatesResourceActionRole()
    {
        var userId = Guid.NewGuid();
        var dto = new ResourceActionRoleDto(1, 2, 3);
        var expectedVm = new ResourceActionRoleVm(10, 1, 2, 3);

        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userReaderMock.Setup(r => r.IsAdminAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _writerMock.Setup(w => w.CreateResourceActionRoleAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVm);

        var handler = new CreateResourceActionRoleCommandHandler(_writerMock.Object, _userReaderMock.Object, _userMock.Object, new Mock<Common.Mediator.IPublisher>().Object);
        var result = await handler.Handle(new CreateResourceActionRoleCommand(dto), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expectedVm));
    }

    [Test]
    public void Handle_NonAdminUser_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();
        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userReaderMock.Setup(r => r.IsAdminAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateResourceActionRoleCommandHandler(_writerMock.Object, _userReaderMock.Object, _userMock.Object, new Mock<Common.Mediator.IPublisher>().Object);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.Handle(new CreateResourceActionRoleCommand(new ResourceActionRoleDto(1, 2, 3)), CancellationToken.None));
    }

    [Test]
    public void Constructor_NullUserId_ThrowsUnauthorizedAccessException()
    {
        _userMock.Setup(u => u.Id).Returns((string?)null);

        Assert.Throws<UnauthorizedAccessException>(() =>
            new CreateResourceActionRoleCommandHandler(_writerMock.Object, _userReaderMock.Object, _userMock.Object, new Mock<Common.Mediator.IPublisher>().Object));
    }
}
