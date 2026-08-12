using Common.Application.Exceptions;
using FluentAssertions;

namespace Common.Application.Tests.Exceptions;

public class ForbiddenAccessExceptionTests
{
    [Fact]
    public void Constructor_CreatesException()
    {
        var ex = new ForbiddenAccessException();
        ex.Should().BeOfType<ForbiddenAccessException>();
        ex.Should().BeAssignableTo<Exception>();
        ex.Message.Should().Be("Insufficient permissions.");
    }

    [Fact]
    public void Constructor_WithResourceAndAction_IncludesRequiredPermission()
    {
        var ex = new ForbiddenAccessException("Users", "Read");

        ex.Resource.Should().Be("Users");
        ex.Action.Should().Be("Read");
        ex.Message.Should().Be("Insufficient permissions. Required permission: Users.Read.");
    }
}
