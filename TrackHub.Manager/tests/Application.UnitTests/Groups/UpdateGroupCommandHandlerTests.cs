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

using TrackHub.Manager.Application.Groups.Commands.Update;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Groups;

[TestFixture]
public class UpdateGroupCommandHandlerTests
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
        var dto = new UpdateGroupDto(1L, "Updated", "New Desc", true);
        var handler = new UpdateGroupCommandHandler(_writerMock.Object);

        await handler.Handle(new UpdateGroupCommand(dto), CancellationToken.None);

        _writerMock.Verify(w => w.UpdateGroupAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_InactiveGroup_PassesActiveFalse()
    {
        var dto = new UpdateGroupDto(5L, "Fleet", "Decommissioned", false);
        var handler = new UpdateGroupCommandHandler(_writerMock.Object);

        await handler.Handle(new UpdateGroupCommand(dto), CancellationToken.None);

        _writerMock.Verify(w => w.UpdateGroupAsync(
            It.Is<UpdateGroupDto>(d => d.Active == false && d.GroupId == 5L),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
