using Common.Application.Exceptions;
using FluentAssertions;

namespace Common.Application.Tests.Exceptions;

public class TooManyRequestsExceptionTests
{
    [Fact]
    public void DefaultConstructor_CreatesException()
    {
        var ex = new TooManyRequestsException();
        ex.Should().BeOfType<TooManyRequestsException>();
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new TooManyRequestsException("Too many requests");
        ex.Message.Should().Be("Too many requests");
    }

    [Fact]
    public void RetryAfterSeconds_CanBeSet()
    {
        var ex = new TooManyRequestsException("limit") { RetryAfterSeconds = 30 };
        ex.RetryAfterSeconds.Should().Be(30);
    }

    [Fact]
    public void RetryAfterSeconds_DefaultIsNull()
    {
        var ex = new TooManyRequestsException();
        ex.RetryAfterSeconds.Should().BeNull();
    }
}
