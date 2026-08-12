// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Accounts.Events;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class AccountCreatedTests
{
    [Test]
    public async Task EventHandler_Calls_CreateAccountSettingsAsync()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var writerMock = new Mock<IAccountSettingsWriter>();
        writerMock.Setup(w => w.CreateAccountSettingsAsync(accountId, CancellationToken.None)).ReturnsAsync(new AccountSettingsVm(accountId, "", "", 0, false, 0));

        var handler = new AccountCreated.Notification.EventHandler(writerMock.Object);

        // Act
        await handler.Handle(new AccountCreated.Notification(accountId), CancellationToken.None);

        // Assert
        writerMock.Verify(w => w.CreateAccountSettingsAsync(accountId, CancellationToken.None), Times.Once);
    }
}
