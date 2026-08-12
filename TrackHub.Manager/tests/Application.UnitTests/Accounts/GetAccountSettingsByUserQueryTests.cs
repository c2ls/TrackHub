// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Accounts.Queries.GetSettings;
using TrackHub.Manager.Domain.Interfaces;
using Common.Application.Interfaces;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class GetAccountSettingsByUserQueryTests
{
    private Mock<IAccountSettingsReader> _settingsReaderMock;
    private Mock<IUserReader> _userReaderMock;
    private Mock<IUser> _userMock;
    private GetAccountSettingsByUserQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _settingsReaderMock = new Mock<IAccountSettingsReader>();
        _userReaderMock = new Mock<IUserReader>();
        _userMock = new Mock<IUser>();

        _userMock.SetupGet(u => u.Id).Returns(Guid.NewGuid().ToString());

        _handler = new GetAccountSettingsByUserQueryHandler(_settingsReaderMock.Object, _userReaderMock.Object, _userMock.Object);
    }

    [Test]
    public async Task Handle_ReturnsSettings_ForCurrentUser()
    {
        var idString = _userMock.Object.Id!;
        var userId = new Guid(idString);
        var userVm = new UserVm(userId, "user", true, Guid.NewGuid());
        _userReaderMock.Setup(r => r.GetUserAsync(userId, CancellationToken.None)).ReturnsAsync(userVm);

        var settingsVm = new AccountSettingsVm(userVm.AccountId, "", "", 0, false, 0);
        _settingsReaderMock.Setup(r => r.GetAccountSettingsAsync(userVm.AccountId, CancellationToken.None)).ReturnsAsync(settingsVm);

        var result = await _handler.Handle(new GetAccountSettingsByUserQuery(), CancellationToken.None);

        Assert.That(result, Is.EqualTo(settingsVm));
    }
}
