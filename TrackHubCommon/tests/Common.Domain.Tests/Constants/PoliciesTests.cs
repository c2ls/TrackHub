using Common.Domain.Constants;
using FluentAssertions;

namespace Common.Domain.Tests.Constants;

public class PoliciesTests
{
    [Theory]
    [InlineData(Policies.AccessPosition, "AccessPosition")]
    [InlineData(Policies.FullAccess, "FullAccess")]
    [InlineData(Policies.ManageAccountSettings, "ManageAccountSettings")]
    [InlineData(Policies.ManageDevices, "ManageDevices")]
    [InlineData(Policies.ManageGroups, "ManageGroups")]
    [InlineData(Policies.ManageUserAccounts, "ManageUserAccounts")]
    public void Policies_HaveExpectedValues(string actual, string expected) =>
        actual.Should().Be(expected);
}
