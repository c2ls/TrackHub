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

using TrackHub.Manager.Application.Operators.Commands.Update;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Operators;

[TestFixture]
public class UpdateOperatorCommandHandlerTests
{
    private Mock<IOperatorWriter> _writerMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<IOperatorWriter>();
    }

    [Test]
    public async Task Handle_ValidCommand_DelegatesToWriter()
    {
        // Arrange
        var dto = new UpdateOperatorDto(Guid.NewGuid(), "Updated", "Desc", "+1", "e@e.com", "Addr", "Contact", 2, 60);
        var command = new UpdateOperatorCommand(dto);
        var handler = new UpdateOperatorCommandHandler(_writerMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _writerMock.Verify(w => w.UpdateOperatorAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_PassesExactDto_ToWriter()
    {
        // Arrange — ensure the exact DTO reference is passed through
        var operatorId = Guid.NewGuid();
        var dto = new UpdateOperatorDto(operatorId, "Name", null, null, null, null, null, 1, 60);
        var handler = new UpdateOperatorCommandHandler(_writerMock.Object);

        // Act
        await handler.Handle(new UpdateOperatorCommand(dto), CancellationToken.None);

        // Assert
        _writerMock.Verify(w => w.UpdateOperatorAsync(
            It.Is<UpdateOperatorDto>(d => d.OperatorId == operatorId && d.Name == "Name"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
