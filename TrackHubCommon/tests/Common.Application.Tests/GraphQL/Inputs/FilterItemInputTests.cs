using Common.Application.GraphQL.Inputs;
using FluentAssertions;

namespace Common.Application.Tests.GraphQL.Inputs;

public class FilterItemInputTests
{
    [Fact]
    public void FilterItemInput_CanSetProperties()
    {
        var item = new FilterItemInput { Key = "name", Value = "test" };
        item.Key.Should().Be("name");
        item.Value.Should().Be("test");
    }

    [Fact]
    public void FilterItemInput_AcceptsVariousValueTypes()
    {
        var intItem = new FilterItemInput { Key = "age", Value = 25 };
        intItem.Value.Should().Be(25);

        var boolItem = new FilterItemInput { Key = "active", Value = true };
        boolItem.Value.Should().Be(true);

        var listItem = new FilterItemInput { Key = "tags", Value = new[] { "a", "b" } };
        listItem.Value.Should().BeOfType<string[]>();
    }
}

public class FiltersInputTests
{
    [Fact]
    public void FiltersInput_DefaultFilters_IsEmpty()
    {
        var input = new FiltersInput();
        input.Filters.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void FiltersInput_CanAddFilters()
    {
        var input = new FiltersInput
        {
            Filters =
            [
                new FilterItemInput { Key = "name", Value = "test" },
                new FilterItemInput { Key = "age", Value = 25 }
            ]
        };
        input.Filters.Should().HaveCount(2);
    }
}
