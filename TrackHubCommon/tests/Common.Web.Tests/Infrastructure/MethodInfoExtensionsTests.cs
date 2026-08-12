using System.Reflection;
using Common.Web.Infrastructure;
using FluentAssertions;

namespace Common.Web.Tests.Infrastructure;

public class MethodInfoExtensionsTests
{
    internal static void NamedMethod() { }

    [Fact]
    public void IsAnonymous_NamedMethod_ReturnsFalse()
    {
        var method = typeof(MethodInfoExtensionsTests).GetMethod(nameof(NamedMethod), BindingFlags.Static | BindingFlags.NonPublic)!;
        method.IsAnonymous().Should().BeFalse();
    }

    [Fact]
    public void IsAnonymous_Lambda_ReturnsTrue()
    {
        Action lambda = () => { };
        lambda.Method.IsAnonymous().Should().BeTrue();
    }

    [Fact]
    public void AnonymousMethod_WithNamedDelegate_DoesNotThrow()
    {
        Delegate del = NamedMethod;
        var act = () => Ardalis.GuardClauses.Guard.Against.AnonymousMethod(del);
        act.Should().NotThrow();
    }

    [Fact]
    public void AnonymousMethod_WithAnonymousDelegate_ThrowsArgumentException()
    {
        Delegate del = () => { };
        var act = () => Ardalis.GuardClauses.Guard.Against.AnonymousMethod(del);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*anonymous*");
    }
}
