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

using TrackHub.Manager.Application.Groups.Commands.Delete;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.Groups;

[TestFixture]
public class DeleteGroupCommandHandlerTests
{
    private Mock<IGroupWriter> _writerMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<IGroupWriter>();
    }

    [Test]
    public async Task Handle_ValidCommand_DelegatesToWriter()
    {
        var groupId = 42L;
        var handler = new DeleteGroupCommandHandler(_writerMock.Object);

        await handler.Handle(new DeleteGroupCommand(groupId), CancellationToken.None);

        _writerMock.Verify(w => w.DeleteGroupAsync(groupId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DifferentGroupId_PassesCorrectId()
    {
        var groupId = 999L;
        var handler = new DeleteGroupCommandHandler(_writerMock.Object);

        await handler.Handle(new DeleteGroupCommand(groupId), CancellationToken.None);

        _writerMock.Verify(w => w.DeleteGroupAsync(999L, It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(w => w.DeleteGroupAsync(42L, It.IsAny<CancellationToken>()), Times.Never);
    }
}
