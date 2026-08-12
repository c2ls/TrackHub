// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Credentials.Command.UpdateToken;
using TrackHub.Manager.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Credentials.Command.UpdateToken;

[TestFixture]
public class UpdateTokenTests
{
    private Mock<ICredentialWriter> _writerMock;
    private Mock<IConfiguration> _configurationMock;
    private UpdateCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _writerMock = new Mock<ICredentialWriter>();
        _configurationMock = new Mock<IConfiguration>();
        _handler = new UpdateCommandHandler(_writerMock.Object, _configurationMock.Object);
    }

    [Test]
    public async Task Handle_Calls_UpdateToken()
    {
        var dto = new UpdateTokenDto(Guid.NewGuid(), "t", DateTimeOffset.UtcNow.AddHours(1), "r", DateTimeOffset.UtcNow.AddDays(1));
        _configurationMock.Setup(c => c["AppSettings:EncryptionKey"]).Returns("key");
        _writerMock.Setup(w => w.UpdateTokenAsync(dto, "key", CancellationToken.None)).Returns(Task.CompletedTask);

        await _handler.Handle(new UpdateTokenCommand(dto), CancellationToken.None);

        _writerMock.Verify(w => w.UpdateTokenAsync(dto, "key", CancellationToken.None), Times.Once);
    }
}
