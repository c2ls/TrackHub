// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Credentials.Command.Update;
using TrackHub.Manager.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Credentials.Command.Update;

[TestFixture]
public class UpdateCredentialTests
{
    private Mock<ICredentialWriter> _writerMock;
    private Mock<IConfiguration> _configurationMock;
    private UpdateCredentialCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _writerMock = new Mock<ICredentialWriter>();
        _configurationMock = new Mock<IConfiguration>();
        _handler = new UpdateCredentialCommandHandler(_writerMock.Object, _configurationMock.Object);
    }

    [Test]
    public async Task Handle_Calls_Update()
    {
        var dto = new UpdateCredentialDto(Guid.NewGuid(), "https://example.com/", "u", "p", "k1", "k2");
        _configurationMock.Setup(c => c["AppSettings:EncryptionKey"]).Returns("key");
        _writerMock.Setup(w => w.UpdateCredentialAsync(dto, It.IsAny<byte[]>(), "key", CancellationToken.None)).Returns(Task.CompletedTask);

        await _handler.Handle(new UpdateCredentialCommand(dto), CancellationToken.None);

        _writerMock.Verify(w => w.UpdateCredentialAsync(dto, It.IsAny<byte[]>(), "key", CancellationToken.None), Times.Once);
    }
}
