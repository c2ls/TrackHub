// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Text.Json;
using HotChocolate.Features;
using HotChocolate.Language;
using HotChocolate.Text.Json;
using HotChocolate.Types;

namespace Common.Application.GraphQL.Types;

public class AnyType : ScalarType<object>
{
    public AnyType() : base("Any") { }

    public override ScalarSerializationType SerializationType
        => ScalarSerializationType.Any;

    public override bool IsValueCompatible(IValueNode valueLiteral)
    {
        ArgumentNullException.ThrowIfNull(valueLiteral);
        return valueLiteral switch
        {
            NullValueNode => true,
            IntValueNode => true,
            FloatValueNode => true,
            BooleanValueNode => true,
            StringValueNode => true,
            _ => false
        };
    }

    public override IValueNode ValueToLiteral(object runtimeValue)
        => runtimeValue is null ? NullValueNode.Default : base.ValueToLiteral(runtimeValue);

    protected override IValueNode OnValueToLiteral(object runtimeValue)
    {
        return runtimeValue switch
        {
            null => NullValueNode.Default,
            int i => new IntValueNode(i),
            long l => new IntValueNode(l),
            float f => new FloatValueNode(f),
            double d => new FloatValueNode(d),
            bool b => new BooleanValueNode(b),
            string s => new StringValueNode(s),
            Guid g => new StringValueNode(g.ToString()),
            _ => throw CreateValueToLiteralError(runtimeValue)
        };
    }

    public override object CoerceInputLiteral(IValueNode valueLiteral)
    {
        ArgumentNullException.ThrowIfNull(valueLiteral);
        return valueLiteral switch
        {
            NullValueNode => null!,
            IntValueNode intValue => intValue.ToInt32(),
            FloatValueNode floatValue => floatValue.ToDouble(),
            BooleanValueNode booleanValue => booleanValue.Value,
            StringValueNode stringValue when Guid.TryParse(stringValue.Value, out var guid) => guid,
            StringValueNode stringValue => stringValue.Value,
            _ => throw new NotSupportedException()
        };
    }

    public override object CoerceInputValue(JsonElement inputValue, IFeatureProvider context)
    {
        return inputValue.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null!,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when Guid.TryParse(inputValue.GetString(), out var guid) => guid,
            JsonValueKind.String => inputValue.GetString()!,
            JsonValueKind.Number when inputValue.TryGetInt64(out var l) => l,
            JsonValueKind.Number => inputValue.GetDouble(),
            _ => throw new NotSupportedException()
        };
    }

    protected override void OnCoerceOutputValue(object runtimeValue, ResultElement resultValue)
    {
        switch (runtimeValue)
        {
            case bool b:
                resultValue.SetBooleanValue(b);
                break;
            case int i:
                resultValue.SetNumberValue(i);
                break;
            case long l:
                resultValue.SetNumberValue(l);
                break;
            case float f:
                resultValue.SetNumberValue(f);
                break;
            case double d:
                resultValue.SetNumberValue(d);
                break;
            case string s:
                resultValue.SetStringValue(s);
                break;
            case Guid g:
                resultValue.SetStringValue(g.ToString());
                break;
            default:
                throw CreateCoerceOutputValueError(runtimeValue);
        }
    }
}
