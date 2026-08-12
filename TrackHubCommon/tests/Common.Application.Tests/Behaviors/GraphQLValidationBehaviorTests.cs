using Common.Application.Behaviors;
using Common.Mediator;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HotChocolate;
using Moq;

namespace Common.Application.Tests.Behaviors;

public class TestGraphQLRequest : IRequest<string> { }

public class GraphQLValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_ProceedsToNext()
    {
        var behavior = new GraphQLValidationBehavior<TestGraphQLRequest, string>([]);
        var result = await behavior.HandleAsync(new TestGraphQLRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_ValidatorsPass_ProceedsToNext()
    {
        var validator = new Mock<IValidator<TestGraphQLRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestGraphQLRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new GraphQLValidationBehavior<TestGraphQLRequest, string>([validator.Object]);
        var result = await behavior.HandleAsync(new TestGraphQLRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsGraphQLException()
    {
        var validator = new Mock<IValidator<TestGraphQLRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestGraphQLRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Field", "Required") }));

        var behavior = new GraphQLValidationBehavior<TestGraphQLRequest, string>([validator.Object]);
        var act = () => behavior.HandleAsync(new TestGraphQLRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        await act.Should().ThrowAsync<GraphQLException>();
    }
}
