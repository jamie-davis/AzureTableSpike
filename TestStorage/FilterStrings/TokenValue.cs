using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using TestStorage.FilterStrings.LexicalAnalysis;

namespace TestStorage.FilterStrings
{
    internal class TokenValue
    {
        private static TokenValue _badTokenValue = new TokenValue();

        public bool IsGood => Value != null;

        public EntityProperty Value { get; private set; }

        public TokenValue()
        {
            Value = null;
        }

        public TokenValue(EntityProperty entityProperty)
        {
            Value = entityProperty;
        }

        public TokenValue(Guid guid)
        {
            Value = new EntityProperty(guid);
        }

        public TokenValue(DateTime dateTime)
        {
            Value = new EntityProperty(dateTime);
        }

        private TokenValue(double doubleValue)
        {
            Value = new EntityProperty(doubleValue);
        }

        private TokenValue(int intValue)
        {
            Value = new EntityProperty(intValue);
        }

        private TokenValue(long longValue)
        {
            Value = new EntityProperty(longValue);
        }

        private TokenValue(string doubleValue)
        {
            Value = new EntityProperty(doubleValue);
        }

        public static TokenValue FromGuid(string value)
        {
            if (!Guid.TryParse(value, out Guid guid))
            {
                return _badTokenValue;
            }

            return new TokenValue(guid);
        }

        public static TokenValue FromDateTime(string value)
        {
            if (!DateTime.TryParse(value, out DateTime dateTime))
            {
                return _badTokenValue;
            }

            return new TokenValue(dateTime);
        }

        public static TokenValue FromDouble(string value)
        {
            if (!double.TryParse(value, out double doubleValue))
            {
                return _badTokenValue;
            }

            return new TokenValue(doubleValue);
        }

        public static TokenValue FromInt(string value)
        {
            if (!int.TryParse(value, out int intValue))
            {
                return _badTokenValue;
            }

            return new TokenValue(intValue);
        }

        public static TokenValue FromLong(string value)
        {
            if (!long.TryParse(value, out long longValue))
            {
                return _badTokenValue;
            }

            return new TokenValue(longValue);
        }

        public static TokenValue FromString(string value)
        {
            return new TokenValue(value);
        }

        public string ValueString()
        {
            switch (Value.PropertyType)
            {
                case EdmType.String:
                    return Value.StringValue ?? "NULL";

                case EdmType.Binary:
                    var bytes = Value.BinaryValue;
                    return bytes == null
                        ? "NULL"
                        : "0x" + string.Join("", bytes.Select(b => b.ToString("X2")));

                case EdmType.Boolean:
                    return Value.BooleanValue == null ? "NULL" : Value.BooleanValue.ToString();

                case EdmType.DateTime:
                    return Value.DateTime == null ? "NULL" : Value.DateTime.Value.ToString("s");

                case EdmType.Double:
                    return Value.DoubleValue == null ? "NULL" : Value.DoubleValue.ToString();

                case EdmType.Guid:
                    return Value.GuidValue == null ? "NULL" : Value.GuidValue.ToString();

                case EdmType.Int32:
                    return Value.Int32Value == null ? "NULL" : Value.Int32Value.ToString();

                case EdmType.Int64:
                    return Value.Int64Value == null ? "NULL" : Value.Int64Value.ToString();

                default:
                    return "Unknown";
            }

        }

        public bool Compare(Token op, TokenValue right, out string error)
        {
            switch (op.TokenType)
            {
                case TokenType.CompEq:
                    return Equal(right.Value, out error);

                case TokenType.CompGt:
                    return Greater(right.Value, out error);

                case TokenType.CompGe:
                    return Greater(right.Value, out error) || Equal(right.Value, out error);

                case TokenType.CompLt:
                    return !Equal(right.Value, out error) && !Greater(right.Value, out error);

                case TokenType.CompLe:
                    return !Greater(right.Value, out error);

                case TokenType.CompNe:
                    return !Equal(right.Value, out error);


                default:
                    error = $@"Invalid compare operator ""{op.Text}""";
                    return false;
            }
        }

        private static bool TryMatchType(EntityProperty rhs, EdmType edmType, out object matchedValue, out string error)
        {
            error = null;
            switch (edmType)
            {
                case EdmType.String:
                    if (rhs.PropertyType == EdmType.String)
                    {
                        matchedValue = rhs.StringValue;
                        return true;
                    }
                    break;

                case EdmType.Binary:
                    if (rhs.PropertyType == EdmType.Binary)
                    {
                        matchedValue = rhs.BinaryValue;
                        return true;
                    }
                    break;

                case EdmType.Boolean:
                    if (rhs.PropertyType == EdmType.Boolean)
                    {
                        matchedValue = rhs.BooleanValue;
                        return true;
                    }
                    break;

                case EdmType.DateTime:
                    if (rhs.PropertyType == EdmType.DateTime)
                    {
                        matchedValue = rhs.DateTime;
                        return true;
                    }
                    break;

                case EdmType.Double:
                    if (rhs.PropertyType == EdmType.Double)
                    {
                        matchedValue = rhs.DoubleValue;
                        return true;
                    }

                    if (rhs.PropertyType == EdmType.Int32)
                    {
                        matchedValue = (double?)rhs.Int32Value;
                        return true;
                    }

                    if (rhs.PropertyType == EdmType.Int64)
                    {
                        matchedValue = (double?)rhs.Int64Value;
                        return true;
                    }
                    break;

                case EdmType.Guid:
                    if (rhs.PropertyType == EdmType.Guid)
                    {
                        matchedValue = rhs.GuidValue;
                        return true;
                    }
                    break;

                case EdmType.Int32:
                    if (rhs.PropertyType == EdmType.Int32)
                    {
                        matchedValue = rhs.Int32Value;
                        return true;
                    }
                    break;

                case EdmType.Int64:
                    if (rhs.PropertyType == EdmType.Int64)
                    {
                        matchedValue = rhs.Int64Value;
                        return true;
                    }
                    if (rhs.PropertyType == EdmType.Int32)
                    {
                        matchedValue = (long?)rhs.Int32Value;
                        return true;
                    }
                    break;

                default:
                    error = "Unknown type conversion.";
                    matchedValue = null;
                    return false;
            }

            error = $"Cannot convert {rhs.PropertyType} to {edmType}";
            matchedValue = null;
            return false;
        }

        private bool Equal(EntityProperty rhs, out string error)
        {
            object matchedValue;
            
            if (!TryMatchType(rhs, Value.PropertyType, out matchedValue, out error))
            {
                return false;
            }

            switch (Value.PropertyType)
            {
                case EdmType.String:
                    return (string)matchedValue == Value.StringValue;

                case EdmType.Binary:
                    return (byte[])matchedValue == Value.BinaryValue;

                case EdmType.Boolean:
                    return (bool?)matchedValue == Value.BooleanValue;

                case EdmType.DateTime:
                    return (DateTime?)matchedValue == Value.DateTime;

                case EdmType.Double:
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    return (double?)matchedValue == Value.DoubleValue;

                case EdmType.Guid:
                    return (Guid?)matchedValue == Value.GuidValue;

                case EdmType.Int32:
                    return (int?)matchedValue == Value.Int32Value;

                case EdmType.Int64:
                    return (long?)matchedValue == Value.Int64Value;
            }

            return false;
        }

        private bool Greater(EntityProperty rhs, out string error)
        {
            object matchedValue;

            if (!TryMatchType(rhs, Value.PropertyType, out matchedValue, out error))
            {
                return false;
            }

            switch (Value.PropertyType)
            {
                case EdmType.String:
                    return string.CompareOrdinal(Value.StringValue, (string)matchedValue) > 0;

                case EdmType.Binary:
                    return GreaterBinary(Value.BinaryValue, (byte[])matchedValue); //find first mismatch, compare that. longer = greater

                case EdmType.Boolean:
                    return (bool?)matchedValue != Value.BooleanValue && ((bool?)matchedValue ?? false);

                case EdmType.DateTime:
                    return Value.DateTime > (DateTime?)matchedValue;

                case EdmType.Double:
                    return Value.DoubleValue > (double?)matchedValue;

                case EdmType.Guid:
                    var guidValue = (Guid?)matchedValue;
                    if (guidValue.HasValue && Value.GuidValue.HasValue)
                    {
                        return GreaterBinary(Value.GuidValue.Value.ToByteArray(), guidValue.Value.ToByteArray());
                    }

                    return guidValue.HasValue;

                case EdmType.Int32:
                    return Value.Int32Value > (int?)matchedValue;

                case EdmType.Int64:
                    return Value.Int64Value > (long?)matchedValue;
            }

            return false;
        }

        private static bool GreaterBinary(byte[] left, byte[] right)
        {
            if (left == null) return right != null;
            if (right == null) return false;

            var firstDIfference = left.Zip(right, (b1, b2) => b1 - b2).FirstOrDefault(b => b != 0);
            if (firstDIfference != 0) return firstDIfference > 0;

            return right.Length > left.Length;
        }
    }
}