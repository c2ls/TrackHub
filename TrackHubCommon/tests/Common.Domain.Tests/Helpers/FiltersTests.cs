using Common.Domain.Helpers;
using FluentAssertions;

namespace Common.Domain.Tests.Helpers;

public class FiltersTests
{
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool Active { get; set; }
    }

    [Fact]
    public void Apply_SingleFilter_FiltersCorrectly()
    {
        var data = new List<TestEntity>
        {
            new() { Name = "Alice", Age = 30, Active = true },
            new() { Name = "Bob", Age = 25, Active = false },
            new() { Name = "Charlie", Age = 30, Active = true }
        }.AsQueryable();

        var filters = new Filters(new Dictionary<string, object> { { "Age", 30 } });
        var result = filters.Apply(data).ToList();

        result.Should().HaveCount(2);
        result.All(x => x.Age == 30).Should().BeTrue();
    }

    [Fact]
    public void Apply_MultipleFilters_FiltersCorrectly()
    {
        var data = new List<TestEntity>
        {
            new() { Name = "Alice", Age = 30, Active = true },
            new() { Name = "Bob", Age = 25, Active = false },
            new() { Name = "Charlie", Age = 30, Active = false }
        }.AsQueryable();

        var filters = new Filters(new Dictionary<string, object> { { "Age", 30 }, { "Active", true } });
        var result = filters.Apply(data).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Alice");
    }

    [Fact]
    public void Apply_EmptyFilters_ReturnsAll()
    {
        var data = new List<TestEntity>
        {
            new() { Name = "Alice", Age = 30, Active = true },
            new() { Name = "Bob", Age = 25, Active = false }
        }.AsQueryable();

        var filters = new Filters([]);
        var result = filters.Apply(data).ToList();
        result.Should().HaveCount(2);
    }

    [Fact]
    public void Apply_NoMatches_ReturnsEmpty()
    {
        var data = new List<TestEntity>
        {
            new() { Name = "Alice", Age = 30, Active = true }
        }.AsQueryable();

        var filters = new Filters(new Dictionary<string, object> { { "Age", 99 } });
        var result = filters.Apply(data).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Apply_StringFilter_FiltersCorrectly()
    {
        var data = new List<TestEntity>
        {
            new() { Name = "Alice", Age = 30, Active = true },
            new() { Name = "Bob", Age = 25, Active = false }
        }.AsQueryable();

        var filters = new Filters(new Dictionary<string, object> { { "Name", "Bob" } });
        var result = filters.Apply(data).ToList();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Bob");
    }

    [Fact]
    public void Constructor_NullDictionary_UsesEmpty()
    {
        var filters = new Filters(null!);
        var data = new List<TestEntity> { new() { Name = "Alice", Age = 30, Active = true } }.AsQueryable();
        var result = filters.Apply(data).ToList();
        result.Should().HaveCount(1);
    }
}
