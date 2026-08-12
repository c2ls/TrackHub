using Common.Application.Behaviors;
using Common.Mediator;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ValidationException = Common.Application.Exceptions.ValidationException;

namespace Common.Application.Tests.Behaviors;

public class TestValidationRequest : IRequest<string> { public string? Name { get; set; } }

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_ProceedsToNext()
    {
        var behavior = new ValidationBehavior<TestValidationRequest, string>([]);
        var result = await behavior.HandleAsync(new TestValidationRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_ValidatorsPass_ProceedsToNext()
    {
        var validator = new Mock<IValidator<TestValidationRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestValidationRequest, string>([validator.Object]);
        var result = await behavior.HandleAsync(new TestValidationRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsValidationException()
    {
        var validator = new Mock<IValidator<TestValidationRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var behavior = new ValidationBehavior<TestValidationRequest, string>([validator.Object]);
        var act = () => behavior.HandleAsync(new TestValidationRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_MultipleValidators_AggregatesFailures()
    {
        var v1 = new Mock<IValidator<TestValidationRequest>>();
        v1.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Error 1") }));

        var v2 = new Mock<IValidator<TestValidationRequest>>();
        v2.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestValidationRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Error 2") }));

        var behavior = new ValidationBehavior<TestValidationRequest, string>([v1.Object, v2.Object]);
        var act = () => behavior.HandleAsync(new TestValidationRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors["Name"].Should().HaveCount(2);
    }
}
