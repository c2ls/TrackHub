using Common.Domain.Constants;
using FluentAssertions;

namespace Common.Domain.Tests.Constants;

public class ActionsTests
{
    [Theory]
    [InlineData(Actions.Edit, "Edit")]
    [InlineData(Actions.Execute, "Execute")]
    [InlineData(Actions.Export, "Export")]
    [InlineData(Actions.Read, "Read")]
    [InlineData(Actions.Write, "Write")]
    [InlineData(Actions.Delete, "Delete")]
    [InlineData(Actions.Custom, "Custom")]
    public void Actions_HaveExpectedValues(string actual, string expected) =>
        actual.Should().Be(expected);
}
