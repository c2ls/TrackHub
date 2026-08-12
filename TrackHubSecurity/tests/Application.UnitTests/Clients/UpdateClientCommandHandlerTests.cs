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
using TrackHub.Security.Application.Clients.Commands.Update;

namespace Application.UnitTests.Clients;

[TestFixture]
public class UpdateClientCommandHandlerTests
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
        var dto = new ClientUserDto(Guid.NewGuid(), Guid.NewGuid());
        var handler = new UpdateClientCommandHandler(_writerMock.Object, _publisherMock.Object, _principalMock.Object);

        await handler.Handle(new UpdateClientCommand(dto), CancellationToken.None);

        _writerMock.Verify(w => w.UpdateClientAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_PassesExactClientUserDto()
    {
        var clientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dto = new ClientUserDto(clientId, userId);
        var handler = new UpdateClientCommandHandler(_writerMock.Object, _publisherMock.Object, _principalMock.Object);

        await handler.Handle(new UpdateClientCommand(dto), CancellationToken.None);

        _writerMock.Verify(w => w.UpdateClientAsync(
            It.Is<ClientUserDto>(d => d.ClientId == clientId && d.UserId == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
