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

using Common.Domain.Enums;
using TrackHub.Manager.Application.Operators.Commands.Delete;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.Operators;

[TestFixture]
public class DeleteOperatorCommandHandlerTests
{
    private Mock<IOperatorWriter> _writerMock;
    private Mock<IOperatorReader> _readerMock;
    private Mock<ICredentialWriter> _credentialWriterMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<IOperatorWriter>();
        _readerMock = new Mock<IOperatorReader>();
        _credentialWriterMock = new Mock<ICredentialWriter>();
    }

    [Test]
    public async Task Handle_OperatorWithCredential_DeletesCredentialThenOperator()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var credentialId = Guid.NewGuid();
        var credential = new CredentialTokenVm(credentialId, "https://api.com", "user", "pass", "salt", null, null, null, null, null, null);
        var operatorVm = new OperatorVm(operatorId, "Op", null, null, null, null, null,
            ProtocolType.CommandTrack, 1, Guid.NewGuid(), DateTimeOffset.UtcNow, credential,
            true, 60, OperatorHealthStatus.Unknown, null, null, null, null, null, null, null, null);

        _readerMock.Setup(r => r.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorVm);

        var handler = new DeleteOperatorCommandHandler(_writerMock.Object, _readerMock.Object, _credentialWriterMock.Object);
        var command = new DeleteOperatorCommand(operatorId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert — credential must be deleted before operator
        _credentialWriterMock.Verify(c => c.DeleteCredentialAsync(credentialId, It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(w => w.DeleteOperatorAsync(operatorId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_OperatorWithoutCredential_SkipsCredentialDeletion()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var operatorVm = new OperatorVm(operatorId, "Op", null, null, null, null, null,
            ProtocolType.CommandTrack, 1, Guid.NewGuid(), DateTimeOffset.UtcNow, null,
            true, 60, OperatorHealthStatus.Unknown, null, null, null, null, null, null, null, null);

        _readerMock.Setup(r => r.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorVm);

        var handler = new DeleteOperatorCommandHandler(_writerMock.Object, _readerMock.Object, _credentialWriterMock.Object);

        // Act
        await handler.Handle(new DeleteOperatorCommand(operatorId), CancellationToken.None);

        // Assert — credential writer should NOT be called
        _credentialWriterMock.Verify(c => c.DeleteCredentialAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _writerMock.Verify(w => w.DeleteOperatorAsync(operatorId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_OperatorWithDefaultCredential_SkipsCredentialDeletion()
    {
        // Arrange — credential is default(CredentialTokenVm), not null
        var operatorId = Guid.NewGuid();
        var operatorVm = new OperatorVm(operatorId, "Op", null, null, null, null, null,
            ProtocolType.CommandTrack, 1, Guid.NewGuid(), DateTimeOffset.UtcNow, default(CredentialTokenVm),
            true, 60, OperatorHealthStatus.Unknown, null, null, null, null, null, null, null, null);

        _readerMock.Setup(r => r.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operatorVm);

        var handler = new DeleteOperatorCommandHandler(_writerMock.Object, _readerMock.Object, _credentialWriterMock.Object);

        // Act
        await handler.Handle(new DeleteOperatorCommand(operatorId), CancellationToken.None);

        // Assert
        _credentialWriterMock.Verify(c => c.DeleteCredentialAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _writerMock.Verify(w => w.DeleteOperatorAsync(operatorId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
