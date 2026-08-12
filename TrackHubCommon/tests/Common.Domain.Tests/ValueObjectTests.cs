using Common.Domain;
using FluentAssertions;

namespace Common.Domain.Tests;

public class ValueObjectTests
{
    private class TestValueObject : ValueObject
    {
        public string Prop1 { get; }
        public int Prop2 { get; }

        public TestValueObject(string prop1, int prop2)
        {
            Prop1 = prop1;
            Prop2 = prop2;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Prop1;
            yield return Prop2;
        }
    }

    private class OtherValueObject : ValueObject
    {
        public string Prop1 { get; }

        public OtherValueObject(string prop1) => Prop1 = prop1;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Prop1;
        }
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var a = new TestValueObject("hello", 42);
        var b = new TestValueObject("hello", 42);
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var a = new TestValueObject("hello", 42);
        var b = new TestValueObject("world", 42);
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var a = new TestValueObject("hello", 42);
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var a = new TestValueObject("hello", 42);
        var b = new OtherValueObject("hello");
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        var a = new TestValueObject("hello", 42);
        var b = new TestValueObject("hello", 42);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_DifferentHash()
    {
        var a = new TestValueObject("hello", 42);
        var b = new TestValueObject("world", 99);
        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void EqualOperator_BothNull_ReturnsTrue()
    {
        // Use reflection to test static protected method
        var method = typeof(ValueObject).GetMethod("EqualOperator",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var result = (bool)method!.Invoke(null, [null, null])!;
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualOperator_LeftNull_ReturnsFalse()
    {
        var method = typeof(ValueObject).GetMethod("EqualOperator",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var b = new TestValueObject("hello", 42);
        var result = (bool)method!.Invoke(null, [null, b])!;
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualOperator_RightNull_ReturnsFalse()
    {
        var method = typeof(ValueObject).GetMethod("EqualOperator",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var a = new TestValueObject("hello", 42);
        var result = (bool)method!.Invoke(null, [a, null])!;
        result.Should().BeFalse();
    }

    [Fact]
    public void NotEqualOperator_DifferentValues_ReturnsTrue()
    {
        var method = typeof(ValueObject).GetMethod("NotEqualOperator",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var a = new TestValueObject("hello", 42);
        var b = new TestValueObject("world", 99);
        var result = (bool)method!.Invoke(null, [a, b])!;
        result.Should().BeTrue();
    }

    [Fact]
    public void NotEqualOperator_SameValues_ReturnsFalse()
    {
        var method = typeof(ValueObject).GetMethod("NotEqualOperator",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var a = new TestValueObject("hello", 42);
        var b = new TestValueObject("hello", 42);
        var result = (bool)method!.Invoke(null, [a, b])!;
        result.Should().BeFalse();
    }
}
