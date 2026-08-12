// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0

using System.Text.Json;
using Common.Application.GraphQL.Types;
using FluentAssertions;
using HotChocolate.Language;
using HotChocolate.Types;

using AnyType = Common.Application.GraphQL.Types.AnyType;

namespace Common.Application.Tests.GraphQL;

public class AnyTypeTests
{
    private readonly AnyType _sut = new();

    [Fact]
    public void Name_Is_Any()
        => _sut.Name.Should().Be("Any");

    [Fact]
    public void RuntimeType_IsObject()
        => _sut.RuntimeType.Should().Be(typeof(object));

    [Fact]
    public void SerializationType_IsAny()
        => _sut.SerializationType.Should().Be(ScalarSerializationType.Any);

    [Theory]
    [InlineData(typeof(NullValueNode))]
    [InlineData(typeof(IntValueNode))]
    [InlineData(typeof(FloatValueNode))]
    [InlineData(typeof(BooleanValueNode))]
    [InlineData(typeof(StringValueNode))]
    public void IsValueCompatible_SupportedTypes_ReturnsTrue(Type nodeType)
    {
        IValueNode node = nodeType.Name switch
        {
            nameof(NullValueNode) => NullValueNode.Default,
            nameof(IntValueNode) => new IntValueNode(42),
            nameof(FloatValueNode) => new FloatValueNode(3.14),
            nameof(BooleanValueNode) => new BooleanValueNode(true),
            nameof(StringValueNode) => new StringValueNode("test"),
            _ => throw new InvalidOperationException()
        };
        _sut.IsValueCompatible(node).Should().BeTrue();
    }

    [Fact]
    public void IsValueCompatible_UnsupportedType_ReturnsFalse()
        => _sut.IsValueCompatible(new ListValueNode([])).Should().BeFalse();

    [Fact]
    public void IsValueCompatible_Null_Throws()
    {
        var act = () => _sut.IsValueCompatible(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValueToLiteral_Null_ReturnsNullValueNode()
        => _sut.ValueToLiteral(null!).Should().Be(NullValueNode.Default);

    [Fact]
    public void ValueToLiteral_Int_ReturnsIntValueNode()
    {
        var result = _sut.ValueToLiteral(42);
        result.Should().BeOfType<IntValueNode>().Which.ToInt32().Should().Be(42);
    }

    [Fact]
    public void ValueToLiteral_Long_ReturnsIntValueNode()
    {
        var result = _sut.ValueToLiteral(42L);
        result.Should().BeOfType<IntValueNode>().Which.ToInt64().Should().Be(42L);
    }

    [Fact]
    public void ValueToLiteral_Float_ReturnsFloatValueNode()
        => _sut.ValueToLiteral(3.14f).Should().BeOfType<FloatValueNode>();

    [Fact]
    public void ValueToLiteral_Double_ReturnsFloatValueNode()
    {
        var result = _sut.ValueToLiteral(3.14d);
        result.Should().BeOfType<FloatValueNode>().Which.ToDouble().Should().Be(3.14d);
    }

    [Fact]
    public void ValueToLiteral_Bool_ReturnsBooleanValueNode()
    {
        var result = _sut.ValueToLiteral(true);
        result.Should().BeOfType<BooleanValueNode>().Which.Value.Should().BeTrue();
    }

    [Fact]
    public void ValueToLiteral_String_ReturnsStringValueNode()
    {
        var result = _sut.ValueToLiteral("hello");
        result.Should().BeOfType<StringValueNode>().Which.Value.Should().Be("hello");
    }

    [Fact]
    public void ValueToLiteral_Guid_ReturnsStringValueNode()
    {
        var guid = Guid.NewGuid();
        var result = _sut.ValueToLiteral(guid);
        result.Should().BeOfType<StringValueNode>().Which.Value.Should().Be(guid.ToString());
    }

    [Fact]
    public void ValueToLiteral_UnsupportedType_Throws()
    {
        var act = () => _sut.ValueToLiteral(new DateTime(2026, 1, 1));
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void CoerceInputLiteral_Null_ReturnsNull()
        => _sut.CoerceInputLiteral(NullValueNode.Default).Should().BeNull();

    [Fact]
    public void CoerceInputLiteral_IntValueNode_ReturnsInt()
        => _sut.CoerceInputLiteral(new IntValueNode(42)).Should().Be(42);

    [Fact]
    public void CoerceInputLiteral_FloatValueNode_ReturnsDouble()
    {
        var result = _sut.CoerceInputLiteral(new FloatValueNode(3.14));
        result.Should().BeOfType<double>().Which.Should().Be(3.14);
    }

    [Fact]
    public void CoerceInputLiteral_BooleanValueNode_ReturnsBool()
        => _sut.CoerceInputLiteral(new BooleanValueNode(true)).Should().Be(true);

    [Fact]
    public void CoerceInputLiteral_StringValueNode_ReturnsString()
        => _sut.CoerceInputLiteral(new StringValueNode("hello")).Should().Be("hello");

    [Fact]
    public void CoerceInputLiteral_StringValueNode_GuidString_ReturnsGuid()
    {
        var guid = Guid.NewGuid();
        _sut.CoerceInputLiteral(new StringValueNode(guid.ToString())).Should().Be(guid);
    }

    [Fact]
    public void CoerceInputLiteral_UnsupportedType_Throws()
    {
        var act = () => _sut.CoerceInputLiteral(new ListValueNode([]));
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void CoerceInputLiteral_Null_Throws()
    {
        var act = () => _sut.CoerceInputLiteral(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("null", null)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("\"hello\"", "hello")]
    [InlineData("42", 42L)]
    public void CoerceInputValue_PrimitiveJson_ReturnsRuntimeValue(string json, object? expected)
    {
        var element = JsonDocument.Parse(json).RootElement;
        var result = _sut.CoerceInputValue(element, null!);
        if (expected is null)
        {
            result.Should().BeNull();
        }
        else
        {
            result.Should().Be(expected);
        }
    }

    [Fact]
    public void CoerceInputValue_NumberDouble_ReturnsDouble()
    {
        var element = JsonDocument.Parse("3.14").RootElement;
        var result = _sut.CoerceInputValue(element, null!);
        result.Should().BeOfType<double>().Which.Should().Be(3.14);
    }

    [Fact]
    public void CoerceInputValue_GuidString_ReturnsGuid()
    {
        var guid = Guid.NewGuid();
        var element = JsonDocument.Parse($"\"{guid}\"").RootElement;
        _sut.CoerceInputValue(element, null!).Should().Be(guid);
    }

    [Fact]
    public void CoerceInputValue_UnsupportedKind_Throws()
    {
        var element = JsonDocument.Parse("[1,2,3]").RootElement;
        var act = () => _sut.CoerceInputValue(element, null!);
        act.Should().Throw<NotSupportedException>();
    }
}
