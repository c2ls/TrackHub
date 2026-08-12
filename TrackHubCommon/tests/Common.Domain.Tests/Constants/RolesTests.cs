using Common.Domain.Constants;
using FluentAssertions;

namespace Common.Domain.Tests.Constants;

public class RolesTests
{
    [Theory]
    [InlineData(Roles.Administrator, "Administrator")]
    [InlineData(Roles.Manager, "Manager")]
    [InlineData(Roles.User, "User")]
    [InlineData(Roles.Audit, "Audit")]
    public void Roles_HaveExpectedValues(string actual, string expected) =>
        actual.Should().Be(expected);
}
