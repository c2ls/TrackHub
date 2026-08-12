// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Credentials.Command.Update;
using TrackHub.Manager.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Credentials.Command.Update;

[TestFixture]
public class UpdateOperatorCredentialTests
{
    private Mock<ICredentialWriter> _writerMock;
    private Mock<IOperatorReader> _operatorReaderMock;
    private Mock<IConfiguration> _configurationMock;
    private UpdateOperatorCredentialCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _writerMock = new Mock<ICredentialWriter>();
        _operatorReaderMock = new Mock<IOperatorReader>();
        _configurationMock = new Mock<IConfiguration>();
        _handler = new UpdateOperatorCredentialCommandHandler(_writerMock.Object, _operatorReaderMock.Object, _configurationMock.Object);
    }

    [Test]
    public async Task Handle_Updates_Credential_Based_On_Operator()
    {
        var operatorId = Guid.NewGuid();
        var dto = new UpdateOperatorCredentialDto(operatorId, "https://example.com/", "u", "p", "k1", "k2");
        var credentialToken = new CredentialTokenVm(Guid.NewGuid(), "https://example.com/", "u", "p", "salt", null, null, null, null, null, null);
        var opVm = new OperatorVm(operatorId, "op", null, null, null, null, null, default, 0, Guid.NewGuid(), DateTimeOffset.UtcNow, credentialToken, true, 60, OperatorHealthStatus.Unknown, null, null, null, null, null, null, null, null);
        _operatorReaderMock.Setup(r => r.GetOperatorAsync(operatorId, CancellationToken.None)).ReturnsAsync(opVm);
        _configurationMock.Setup(c => c["AppSettings:EncryptionKey"]).Returns("key");
        _writerMock.Setup(w => w.UpdateCredentialAsync(It.IsAny<UpdateCredentialDto>(), It.IsAny<byte[]>(), "key", CancellationToken.None)).Returns(Task.CompletedTask);

        await _handler.Handle(new UpdateOperatorCredentialCommand(dto), CancellationToken.None);

        _writerMock.Verify(w => w.UpdateCredentialAsync(It.IsAny<UpdateCredentialDto>(), It.IsAny<byte[]>(), "key", CancellationToken.None), Times.Once);
    }
}
