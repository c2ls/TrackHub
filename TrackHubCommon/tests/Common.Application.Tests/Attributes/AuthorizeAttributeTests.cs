using Common.Application.Attributes;
using FluentAssertions;

namespace Common.Application.Tests.Attributes;

public class AuthorizeAttributeTests
{
    [Fact]
    public void DefaultConstructor_HasEmptyResourceAndAction()
    {
        var attr = new AuthorizeAttribute();
        attr.Resource.Should().BeEmpty();
        attr.Action.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var attr = new AuthorizeAttribute { Resource = "Users", Action = "Read" };
        attr.Resource.Should().Be("Users");
        attr.Action.Should().Be("Read");
    }
}
