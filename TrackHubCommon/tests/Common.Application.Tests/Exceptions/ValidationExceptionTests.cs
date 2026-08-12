using Common.Application.Exceptions;
using FluentAssertions;
using FluentValidation.Results;

namespace Common.Application.Tests.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessage()
    {
        var ex = new ValidationException();
        ex.Message.Should().Be("One or more validation failures have occurred.");
        ex.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithFailures_GroupsByPropertyName()
    {
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Name", "Name must be at least 3 chars"),
            new("Email", "Email is required")
        };

        var ex = new ValidationException(failures);

        ex.Errors.Should().HaveCount(2);
        ex.Errors["Name"].Should().HaveCount(2);
        ex.Errors["Name"].Should().Contain("Name is required");
        ex.Errors["Name"].Should().Contain("Name must be at least 3 chars");
        ex.Errors["Email"].Should().HaveCount(1);
        ex.Errors["Email"].Should().Contain("Email is required");
    }

    [Fact]
    public void Constructor_WithEmptyFailures_HasEmptyErrors()
    {
        var ex = new ValidationException([]);
        ex.Errors.Should().BeEmpty();
    }
}
