using Common.Application.Extensions;
using Common.Application.GraphQL.Inputs;
using FluentAssertions;

namespace Common.Application.Tests.Extensions;

public class FiltersExtensionsTests
{
    [Fact]
    public void GetFilters_ConvertsFiltersInputToFilters()
    {
        var input = new FiltersInput
        {
            Filters =
            [
                new FilterItemInput { Key = "Name", Value = "Test" },
                new FilterItemInput { Key = "Age", Value = 25 }
            ]
        };

        var result = input.GetFilters();
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetFilters_EmptyFilters_ReturnsFiltersWithNoEntries()
    {
        var input = new FiltersInput { Filters = [] };
        var result = input.GetFilters();
        result.Should().NotBeNull();
    }
}
