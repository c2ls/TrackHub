// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.CredentialToken.Queries.GetToken;
using TrackHub.Manager.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.UnitTests.Credentials.Queries.GetToken;

[TestFixture]
public class GetTokenTests
{
    private Mock<ICredentialReader> _readerMock;
    private Mock<IConfiguration> _configurationMock;
    private GetTokenQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _readerMock = new Mock<ICredentialReader>();
        _configurationMock = new Mock<IConfiguration>();
        _handler = new GetTokenQueryHandler(_readerMock.Object, _configurationMock.Object);
    }

    [Test]
    public async Task Handle_Returns_TokenVm()
    {
        var id = Guid.NewGuid();
        var vm = new TokenVm("token", DateTimeOffset.UtcNow.AddHours(1), "refresh", DateTimeOffset.UtcNow.AddDays(1));
        var key = "key";
        _configurationMock.Setup(c => c["AppSettings:EncryptionKey"]).Returns(key);
        _readerMock.Setup(r => r.GetTokenAsync(id, key, CancellationToken.None)).ReturnsAsync(vm);

        var result = await _handler.Handle(new GetTokenQuery(id), CancellationToken.None);

        Assert.That(result, Is.EqualTo(vm));
    }
}
