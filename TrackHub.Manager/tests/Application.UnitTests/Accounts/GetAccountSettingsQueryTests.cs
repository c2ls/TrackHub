// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Accounts.Queries.GetSettings;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class GetAccountSettingsQueryTests
{
    private Mock<IAccountSettingsReader> _readerMock;
    private GetAccountSettingsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _readerMock = new Mock<IAccountSettingsReader>();
        _handler = new GetAccountSettingsQueryHandler(_readerMock.Object);
    }

    [Test]
    public async Task Handle_ReturnsSettings()
    {
        var id = Guid.NewGuid();
        var vm = new AccountSettingsVm(id, "", "", 0, false, 0);
        _readerMock.Setup(r => r.GetAccountSettingsAsync(id, CancellationToken.None)).ReturnsAsync(vm);

        var result = await _handler.Handle(new GetAccountSettingsQuery(id), CancellationToken.None);

        Assert.That(result, Is.EqualTo(vm));
    }
}
