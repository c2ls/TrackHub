using Common.Application.Attributes;
using Common.Application.Behaviors;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Enums;
using Common.Mediator;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Common.Application.Tests.Behaviors;

public class AccountStatusBehaviorTests
{
    private readonly Mock<IAccountOperationalStatusService> _statusServiceMock = new();
    private readonly Mock<ICurrentPrincipal> _principalMock = new();
    private readonly Mock<ILogger<AccountStatusBehavior<AccountScopedRequest, string>>> _scopedLoggerMock = new();
    private readonly Mock<ILogger<AccountStatusBehavior<PlainRequest, string>>> _plainLoggerMock = new();
    private readonly Mock<ILogger<AccountStatusBehavior<AllowSuspendedRequest, string>>> _allowLoggerMock = new();

    public class PlainRequest : IRequest<string> { }

    public class AccountScopedRequest : IRequest<string>
    {
        public Guid AccountId { get; init; }
    }

    [AllowSuspendedAccount]
    public class AllowSuspendedRequest : IRequest<string>
    {
        public Guid AccountId { get; init; }
    }

    [Fact]
    public async Task Handle_AllowSuspendedAccount_ProceedsEvenWhenSuspended()
    {
        var accountId = Guid.NewGuid();
        var behavior = new AccountStatusBehavior<AllowSuspendedRequest, string>(
            _statusServiceMock.Object, _principalMock.Object, _allowLoggerMock.Object);

        var result = await behavior.HandleAsync(
            new AllowSuspendedRequest { AccountId = accountId }, () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
        // The status service is never consulted for an allow-listed request.
        _statusServiceMock.Verify(s => s.GetStatusAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoAccountResolvable_ProceedsToNext()
    {
        _principalMock.Setup(p => p.AccountId).Returns((Guid?)null);
        var behavior = new AccountStatusBehavior<PlainRequest, string>(
            _statusServiceMock.Object, _principalMock.Object, _plainLoggerMock.Object);

        var result = await behavior.HandleAsync(new PlainRequest(), () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
        _statusServiceMock.Verify(s => s.GetStatusAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(AccountStatus.Suspended)]
    [InlineData(AccountStatus.Cancelled)]
    [InlineData(AccountStatus.Archived)]
    public async Task Handle_NonOperationalAccount_ThrowsAccountSuspended(AccountStatus status)
    {
        var accountId = Guid.NewGuid();
        _statusServiceMock.Setup(s => s.GetStatusAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(status);
        var behavior = new AccountStatusBehavior<AccountScopedRequest, string>(
            _statusServiceMock.Object, _principalMock.Object, _scopedLoggerMock.Object);

        var act = () => behavior.HandleAsync(
            new AccountScopedRequest { AccountId = accountId }, () => Task.FromResult("OK"), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<AccountSuspendedException>();
        exception.Which.AccountId.Should().Be(accountId);
        exception.Which.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(AccountStatus.Trial)]
    [InlineData(AccountStatus.Active)]
    public async Task Handle_OperationalAccount_ProceedsToNext(AccountStatus status)
    {
        var accountId = Guid.NewGuid();
        _statusServiceMock.Setup(s => s.GetStatusAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(status);
        var behavior = new AccountStatusBehavior<AccountScopedRequest, string>(
            _statusServiceMock.Object, _principalMock.Object, _scopedLoggerMock.Object);

        var result = await behavior.HandleAsync(
            new AccountScopedRequest { AccountId = accountId }, () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_UnknownAccount_NullStatus_ProceedsToNext()
    {
        var accountId = Guid.NewGuid();
        _statusServiceMock.Setup(s => s.GetStatusAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountStatus?)null);
        var behavior = new AccountStatusBehavior<AccountScopedRequest, string>(
            _statusServiceMock.Object, _principalMock.Object, _scopedLoggerMock.Object);

        var result = await behavior.HandleAsync(
            new AccountScopedRequest { AccountId = accountId }, () => Task.FromResult("OK"), CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_ResolvesAccountFromPrincipal_WhenRequestHasNoAccountId()
    {
        var accountId = Guid.NewGuid();
        _principalMock.Setup(p => p.AccountId).Returns(accountId);
        _statusServiceMock.Setup(s => s.GetStatusAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(AccountStatus.Suspended);
        var behavior = new AccountStatusBehavior<PlainRequest, string>(
            _statusServiceMock.Object, _principalMock.Object, _plainLoggerMock.Object);

        var act = () => behavior.HandleAsync(new PlainRequest(), () => Task.FromResult("OK"), CancellationToken.None);

        await act.Should().ThrowAsync<AccountSuspendedException>();
    }
}
