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
using Microsoft.Extensions.Configuration;
using TrackHub.Security.Application.Clients.Commands.Create;

namespace Application.UnitTests.Clients;

[TestFixture]
public class CreateClientCommandHandlerTests
{
    private Mock<IClientWriter> _writerMock;
    private Mock<IConfiguration> _configMock;
    private Mock<IPublisher> _publisherMock;
    private Mock<ICurrentPrincipal> _principalMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<IClientWriter>();
        _configMock = new Mock<IConfiguration>();
        _publisherMock = new Mock<IPublisher>();
        _principalMock = new Mock<ICurrentPrincipal>();
    }

    [Test]
    public async Task Handle_ValidCommand_CreatesClientWithSaltAndKey()
    {
        // Arrange
        var clientDto = new ClientDto(Guid.NewGuid(), "MyClient", "Description", "secret123");
        var expectedVm = new ClientVm(Guid.NewGuid(), clientDto.UserId, "MyClient", "Description", "encrypted", true, DateTimeOffset.UtcNow);

        _configMock.Setup(c => c["AppSettings:EncryptionKey"]).Returns("test-encryption-key-32chars!!");
        _writerMock.Setup(w => w.CreateClientAsync(clientDto, It.IsAny<byte[]>(), "test-encryption-key-32chars!!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVm);

        var handler = new CreateClientCommandHandler(_writerMock.Object, _configMock.Object, _publisherMock.Object, _principalMock.Object);

        // Act
        var result = await handler.Handle(new CreateClientCommand(clientDto), CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(expectedVm));
        _writerMock.Verify(w => w.CreateClientAsync(
            clientDto,
            It.Is<byte[]>(s => s.Length > 0),
            "test-encryption-key-32chars!!",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_MissingEncryptionKey_ThrowsException()
    {
        // Arrange — config returns null for encryption key
        _configMock.Setup(c => c["AppSettings:EncryptionKey"]).Returns((string?)null);

        var handler = new CreateClientCommandHandler(_writerMock.Object, _configMock.Object, _publisherMock.Object, _principalMock.Object);
        var clientDto = new ClientDto(null, "Client", "Desc", "secret");

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await handler.Handle(new CreateClientCommand(clientDto), CancellationToken.None));
    }

    [Test]
    public async Task Handle_GeneratesUniqueSaltPerCall()
    {
        // Arrange
        var clientDto = new ClientDto(null, "Client", "Desc", "secret");
        byte[]? firstSalt = null;
        byte[]? secondSalt = null;

        _configMock.Setup(c => c["AppSettings:EncryptionKey"]).Returns("key12345678901234567890123456!!");
        _writerMock.Setup(w => w.CreateClientAsync(clientDto, It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<ClientDto, byte[], string, CancellationToken>((_, salt, _, _) =>
            {
                if (firstSalt == null) firstSalt = salt.ToArray();
                else secondSalt = salt.ToArray();
            })
            .ReturnsAsync(new ClientVm(Guid.NewGuid(), null, "Client", "Desc", "enc", true, DateTimeOffset.UtcNow));

        var handler = new CreateClientCommandHandler(_writerMock.Object, _configMock.Object, _publisherMock.Object, _principalMock.Object);

        // Act
        await handler.Handle(new CreateClientCommand(clientDto), CancellationToken.None);
        await handler.Handle(new CreateClientCommand(clientDto), CancellationToken.None);

        // Assert — each call should produce a different salt
        Assert.That(firstSalt, Is.Not.Null);
        Assert.That(secondSalt, Is.Not.Null);
        Assert.That(firstSalt, Is.Not.EqualTo(secondSalt));
    }
}
