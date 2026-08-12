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
using TrackHub.Security.Application.Clients.Commands.Delete;

namespace Application.UnitTests.Clients;

[TestFixture]
public class DeleteClientCommandHandlerTests
{
    private Mock<IClientWriter> _writerMock;
    private Mock<IPublisher> _publisherMock;
    private Mock<ICurrentPrincipal> _principalMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<IClientWriter>();
        _publisherMock = new Mock<IPublisher>();
        _principalMock = new Mock<ICurrentPrincipal>();
    }

    [Test]
    public async Task Handle_ValidCommand_DelegatesToWriter()
    {
        var clientId = Guid.NewGuid();
        var handler = new DeleteClientCommandHandler(_writerMock.Object, _publisherMock.Object, _principalMock.Object);

        await handler.Handle(new DeleteClientCommand(clientId), CancellationToken.None);

        _writerMock.Verify(w => w.DeleteClientAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DifferentId_PassesCorrectId()
    {
        var clientId = Guid.NewGuid();
        var wrongId = Guid.NewGuid();
        var handler = new DeleteClientCommandHandler(_writerMock.Object, _publisherMock.Object, _principalMock.Object);

        await handler.Handle(new DeleteClientCommand(clientId), CancellationToken.None);

        _writerMock.Verify(w => w.DeleteClientAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(w => w.DeleteClientAsync(wrongId, It.IsAny<CancellationToken>()), Times.Never);
    }
}
