// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Credentials.Queries.GetByOperator;
using TrackHub.Manager.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.UnitTests.Credentials.Queries.GetByOperator;

[TestFixture]
public class GetCredentialByOperatorTests
{
    private Mock<ICredentialReader> _readerMock;
    private Mock<IConfiguration> _configurationMock;
    private GetCredentialsByOperatorQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _readerMock = new Mock<ICredentialReader>();
        _configurationMock = new Mock<IConfiguration>();
        _handler = new GetCredentialsByOperatorQueryHandler(_readerMock.Object, _configurationMock.Object);
    }

    [Test]
    public async Task Handle_Returns_CredentialVm()
    {
        var operatorId = Guid.NewGuid();
        var vm = new CredentialVm(Guid.NewGuid(), "https://example.com/", "u", "p", null, null);
        var key = "key";
        _configurationMock.Setup(c => c["AppSettings:EncryptionKey"]).Returns(key);
        _readerMock.Setup(r => r.GetCredentialByOperatorAsync(operatorId, key, CancellationToken.None)).ReturnsAsync(vm);

        var result = await _handler.Handle(new GetCredentialByOperatorQuery(operatorId), CancellationToken.None);

        Assert.That(result, Is.EqualTo(vm));
    }
}
