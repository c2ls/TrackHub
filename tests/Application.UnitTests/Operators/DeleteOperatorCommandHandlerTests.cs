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

using TrackHub.Manager.Application.Operators.Commands.Delete;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.Operators;

[TestFixture]
public class DeleteOperatorCommandHandlerTests
{
    private Mock<IOperatorWriter> _writerMock;
    private Mock<ICredentialWriter> _credentialWriterMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<IOperatorWriter>();
        _credentialWriterMock = new Mock<ICredentialWriter>();
    }

    // The credential cleanup must run unconditionally and by OPERATOR id: the operator VM
    // redacts the credential for callers without Credentials/Custom, so any handler logic
    // that inspects the VM to decide whether a credential exists deletes nothing for those
    // callers and the operator delete dies on the credentials FK.
    [Test]
    public async Task Handle_DeletesCredentialByOperatorBeforeOperator()
    {
        var operatorId = Guid.NewGuid();
        var sequence = new List<string>();
        _credentialWriterMock
            .Setup(c => c.DeleteCredentialByOperatorAsync(operatorId, It.IsAny<CancellationToken>()))
            .Callback(() => sequence.Add("credential"))
            .Returns(Task.CompletedTask);
        _writerMock
            .Setup(w => w.DeleteOperatorAsync(operatorId, It.IsAny<CancellationToken>()))
            .Callback(() => sequence.Add("operator"))
            .Returns(Task.CompletedTask);

        var handler = new DeleteOperatorCommandHandler(_writerMock.Object, _credentialWriterMock.Object);

        await handler.Handle(new DeleteOperatorCommand(operatorId), CancellationToken.None);

        Assert.That(sequence, Is.EqualTo(new[] { "credential", "operator" }));
    }

    [Test]
    public async Task Handle_AlwaysCallsCredentialCleanup_EvenWhenCallerCannotViewCredentials()
    {
        // No reader involved at all: whether the caller may view credential material is
        // irrelevant to cleanup. The writer no-ops when the operator has no credential.
        var operatorId = Guid.NewGuid();
        var handler = new DeleteOperatorCommandHandler(_writerMock.Object, _credentialWriterMock.Object);

        await handler.Handle(new DeleteOperatorCommand(operatorId), CancellationToken.None);

        _credentialWriterMock.Verify(c => c.DeleteCredentialByOperatorAsync(operatorId, It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(w => w.DeleteOperatorAsync(operatorId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
