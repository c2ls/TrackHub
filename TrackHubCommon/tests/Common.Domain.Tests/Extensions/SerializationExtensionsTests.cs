using System.Text.Json;
using Common.Domain.Extensions;
using FluentAssertions;

namespace Common.Domain.Tests.Extensions;

public class SerializationExtensionsTests
{
    private class SampleDto
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsObject()
    {
        var json = """{"name":"Test","value":42}""";
        var result = json.Deserialize<SampleDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Deserialize_CaseInsensitive_ReturnsObject()
    {
        var json = """{"NAME":"Test","VALUE":42}""";
        var result = json.Deserialize<SampleDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }

    [Fact]
    public void Deserialize_WithCustomOptions_ReturnsObject()
    {
        var json = """{"Name":"Custom","Value":100}""";
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = false };
        var result = json.Deserialize<SampleDto>(options);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Custom");
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsException()
    {
        var json = "not valid json";
        var act = () => json.Deserialize<SampleDto>();
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_EmptyObject_ReturnsDefaultValues()
    {
        var json = "{}";
        var result = json.Deserialize<SampleDto>();
        result.Should().NotBeNull();
        result!.Name.Should().BeNull();
        result.Value.Should().Be(0);
    }
}
