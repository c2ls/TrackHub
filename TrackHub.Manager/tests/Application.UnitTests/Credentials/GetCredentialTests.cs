// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Credentials.Queries.Get;
using TrackHub.Manager.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.UnitTests.Credentials.Queries.Get;

[TestFixture]
public class GetCredentialTests
{
    private Mock<ICredentialReader> _readerMock;
    private Mock<IConfiguration> _configurationMock;
    private GetCredentialsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _readerMock = new Mock<ICredentialReader>();
        _configurationMock = new Mock<IConfiguration>();
        _handler = new GetCredentialsQueryHandler(_readerMock.Object, _configurationMock.Object);
    }

    [Test]
    public async Task Handle_Returns_CredentialVm()
    {
        var id = Guid.NewGuid();
        var vm = new CredentialVm(id, "https://example.com/", "u", "p", null, null);
        var key = "key";
        _configurationMock.Setup(c => c["AppSettings:EncryptionKey"]).Returns(key);
        _readerMock.Setup(r => r.GetCredentialAsync(id, key, CancellationToken.None)).ReturnsAsync(vm);

        var result = await _handler.Handle(new GetCredentialQuery(id), CancellationToken.None);

        Assert.That(result, Is.EqualTo(vm));
    }
}
