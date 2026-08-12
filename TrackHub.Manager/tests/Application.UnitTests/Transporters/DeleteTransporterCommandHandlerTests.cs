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

using TrackHub.Manager.Application.Transporters.Commands.Delete;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.Transporters;

[TestFixture]
public class DeleteTransporterCommandHandlerTests
{
    private Mock<ITransporterWriter> _writerMock;
    private Mock<ITransporterPositionWriter> _positionWriterMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<ITransporterWriter>();
        _positionWriterMock = new Mock<ITransporterPositionWriter>();
    }

    [Test]
    public async Task Handle_ValidCommand_DeletesPositionThenTransporter()
    {
        // Arrange
        var transporterId = Guid.NewGuid();
        var callOrder = new List<string>();

        _positionWriterMock.Setup(p => p.DeleteTransporterPositionAsync(transporterId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("position"))
            .Returns(Task.CompletedTask);
        _writerMock.Setup(w => w.DeleteTransporterAsync(transporterId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("transporter"))
            .Returns(Task.CompletedTask);

        var handler = new DeleteTransporterCommandHandler(_writerMock.Object, _positionWriterMock.Object);

        // Act
        await handler.Handle(new DeleteTransporterCommand(transporterId), CancellationToken.None);

        // Assert — position must be deleted BEFORE transporter (cascade order matters)
        Assert.That(callOrder, Is.EqualTo(new[] { "position", "transporter" }));
        _positionWriterMock.Verify(p => p.DeleteTransporterPositionAsync(transporterId, It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(w => w.DeleteTransporterAsync(transporterId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_AlwaysDeletesBoth_EvenIfPositionDoesNotExist()
    {
        // Arrange — position delete succeeds (no-op if not found)
        var transporterId = Guid.NewGuid();

        var handler = new DeleteTransporterCommandHandler(_writerMock.Object, _positionWriterMock.Object);

        // Act
        await handler.Handle(new DeleteTransporterCommand(transporterId), CancellationToken.None);

        // Assert — both methods must be called
        _positionWriterMock.Verify(p => p.DeleteTransporterPositionAsync(transporterId, It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(w => w.DeleteTransporterAsync(transporterId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
