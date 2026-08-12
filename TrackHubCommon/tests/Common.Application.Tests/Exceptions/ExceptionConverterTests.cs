using Common.Application.Exceptions;
using FluentAssertions;
using FluentValidation.Results;
using GraphQL;

namespace Common.Application.Tests.Exceptions;

public class ExceptionConverterTests
{
    private static GraphQLError CreateGraphQLError(string message)
        => new() { Message = message };

    [Fact]
    public void ConvertToIError_GraphQLErrors_ConvertsAll()
    {
        var errors = new List<GraphQLError>
        {
            CreateGraphQLError("Error 1"),
            CreateGraphQLError("Error 2")
        };

        var result = errors.ConvertToIError();
        result.Should().HaveCount(2);
        result[0].Message.Should().Be("Error 1");
        result[1].Message.Should().Be("Error 2");
    }

    [Fact]
    public void ConvertToIError_ValidationFailures_ConvertsAll()
    {
        var failures = new List<ValidationFailure>
        {
            new("Prop1", "Error 1"),
            new("Prop2", "Error 2")
        };

        var result = failures.ConvertToIError();
        result.Should().HaveCount(2);
        result[0].Message.Should().Be("Error 1");
        result[1].Message.Should().Be("Error 2");
    }

    [Fact]
    public void ConvertToIError_SingleGraphQLError_Converts()
    {
        var error = CreateGraphQLError("Test error");
        var result = error.ConvertToIError();
        result.Message.Should().Be("Test error");
    }

    [Fact]
    public void ConvertToIError_SingleValidationFailure_Converts()
    {
        var failure = new ValidationFailure("Property", "Must be valid");
        var result = failure.ConvertToIError();
        result.Message.Should().Be("Must be valid");
    }

    [Fact]
    public void ConvertToIError_GraphQLError_WithPathAndLocations_PropagatesToError()
    {
        var error = new GraphQLError
        {
            Message = "boom",
            Path = new ErrorPath(new object[] { "user", "name" }),
            Locations =
            [
                new GraphQLLocation { Line = 3, Column = 7 }
            ]
        };

        var result = error.ConvertToIError();

        result.Message.Should().Be("boom");
        result.Path.Should().NotBeNull();
        result.Path!.ToList().Should().ContainInOrder("user", "name");
        result.Locations.Should().NotBeNull();
        result.Locations!.Should().ContainSingle()
            .Which.Should().Match<HotChocolate.Location>(l => l.Line == 3 && l.Column == 7);
    }

    [Fact]
    public void ConvertToIError_GraphQLError_WithoutOptionalFields_HasNoPathOrLocations()
    {
        var error = new GraphQLError { Message = "naked" };

        var result = error.ConvertToIError();

        result.Message.Should().Be("naked");
        result.Path.Should().BeNull();
        (result.Locations is null || result.Locations.Count == 0).Should().BeTrue();
    }
}
