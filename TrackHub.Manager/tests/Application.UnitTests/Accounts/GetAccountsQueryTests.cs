// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Accounts.Queries.GetAll;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class GetAccountsQueryTests
{
    private Mock<IAccountReader> _readerMock;
    private GetAccountsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _readerMock = new Mock<IAccountReader>();
        _handler = new GetAccountsQueryHandler(_readerMock.Object);
    }

    [Test]
    public async Task Handle_ReturnsAccountsPage()
    {
        var list = new List<AccountVm> { new(Guid.NewGuid(), "A", null, default, 1, Common.Domain.Enums.AccountStatus.Active, 2, true, DateTimeOffset.UtcNow) };
        var page = new AccountsPageVm(list, 1);
        _readerMock.Setup(r => r.GetAccountsAsync(0, 50, null, CancellationToken.None)).ReturnsAsync(page);

        var result = await _handler.Handle(new GetAccountsQuery(null, null, null), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Is.EqualTo(list));
            Assert.That(result.TotalCount, Is.EqualTo(1));
        });
    }

    // An omitted page window must not mean "everything": it resolves to the shared default, and an
    // over-large take is clamped rather than honoured.
    [Test]
    public async Task Handle_ClampsPageWindow()
    {
        _readerMock
            .Setup(r => r.GetAccountsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), CancellationToken.None))
            .ReturnsAsync(new AccountsPageVm([], 0));

        await _handler.Handle(new GetAccountsQuery(-5, 10_000, "acme"), CancellationToken.None);

        _readerMock.Verify(r => r.GetAccountsAsync(0, 500, "acme", CancellationToken.None), Times.Once);
    }
}
