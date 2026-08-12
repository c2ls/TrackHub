// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Accounts.Commands.Create;
using TrackHub.Manager.Application.Accounts.Events;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class CreateAccountCommandHandlerTests
{
    private Mock<IAccountWriter> _accountWriterMock;
    private Mock<ISecurityWriter> _securityWriterMock;
    private Mock<IPublisher> _publisherMock;
    private CreateAccountCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _accountWriterMock = new Mock<IAccountWriter>();
        _securityWriterMock = new Mock<ISecurityWriter>();
        _publisherMock = new Mock<IPublisher>();

        _handler = new CreateAccountCommandHandler(
            _accountWriterMock.Object,
            _securityWriterMock.Object,
            _publisherMock.Object);
    }

    [Test]
    public async Task Handle_WhenCalled_CreatesAccountPublishesAndCreatesUser()
    {
        // Arrange
        var accountDto = new AccountDto(
            "Acme",
            "Description",
            (short)1,
            true,
            "P@ssw0rd",
            "admin@acme.local",
            "John",
            "Doe");

        var accountVm = new AccountVm(
            Guid.NewGuid(),
            accountDto.Name,
            accountDto.Description,
            default,
            accountDto.TypeId,
            Common.Domain.Enums.AccountStatus.Active,
            2,
            accountDto.Active,
            DateTimeOffset.UtcNow);

        _accountWriterMock.Setup(w => w.CreateAccountAsync(It.IsAny<AccountDto>(), CancellationToken.None))
            .ReturnsAsync(accountVm);

        _securityWriterMock.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>(), CancellationToken.None))
            .ReturnsAsync(new UserVm(Guid.NewGuid(), "user", true, accountVm.AccountId));

        var command = new CreateAccountCommand(accountDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(accountVm));
        _publisherMock.Verify(p => p.Publish(It.IsAny<AccountCreated.Notification>(), CancellationToken.None), Times.Once);
        _securityWriterMock.Verify(s => s.CreateUserAsync(It.Is<CreateUserDto>(d => d.AccountId == accountVm.AccountId && d.EmailAddress == accountDto.EmailAddress && d.Password == accountDto.Password), CancellationToken.None), Times.Once);
    }
}
