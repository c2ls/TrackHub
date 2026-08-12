using Common.Application.Behaviors;
using Common.Mediator;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Common.Application.Tests.Behaviors;

public class TestUnhandledRequest : IRequest<string> { }

public class UnhandledExceptionBehaviorTests
{
    [Fact]
    public async Task Handle_NoException_ReturnsResult()
    {
        var logger = NullLogger<TestUnhandledRequest>.Instance;
        var behavior = new UnhandledExceptionBehavior<TestUnhandledRequest, string>(logger);
        var result = await behavior.HandleAsync(new TestUnhandledRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_WithException_LogsAndRethrows()
    {
        var logger = NullLogger<TestUnhandledRequest>.Instance;
        var behavior = new UnhandledExceptionBehavior<TestUnhandledRequest, string>(logger);
        var act = () => behavior.HandleAsync(
            new TestUnhandledRequest(),
            () => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }
}
