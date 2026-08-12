// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Accounts.Queries.Get;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class GetAccountQueryTests
{
    private Mock<IAccountReader> _readerMock;
    private GetAccountQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _readerMock = new Mock<IAccountReader>();
        _handler = new GetAccountQueryHandler(_readerMock.Object);
    }

    [Test]
    public async Task Handle_WhenAccountExists_ReturnsAccountVm()
    {
        var id = Guid.NewGuid();
        var vm = new AccountVm(id, "Name", null, default, 1, Common.Domain.Enums.AccountStatus.Active, 2, true, DateTimeOffset.UtcNow);
        _readerMock.Setup(r => r.GetAccountAsync(id, CancellationToken.None)).ReturnsAsync(vm);

        var result = await _handler.Handle(new GetAccountQuery(id), CancellationToken.None);

        Assert.That(result, Is.EqualTo(vm));
    }
}
