using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using TestStorage.FilterStrings.LexicalAnalysis;

namespace TestStorage.FilterStrings
{
    internal static class TokenValueLoader
    {
        public static TokenValue Load(Token left, IDictionary<string, EntityProperty> data)
        {
            if (!left.IsData())
                return null;

            if (left.TokenType == TokenType.GuidLiteral)
            {
                return TokenValue.FromGuid(left.Text);
            }

            if (left.TokenType == TokenType.DateTimeLiteral)
            {
                return TokenValue.FromDateTime(left.Text);
            }

            if (left.TokenType == TokenType.DoubleLiteral)
            {
                return TokenValue.FromDouble(left.Text);
            }

            if (left.TokenType == TokenType.IntLiteral)
            {
                return TokenValue.FromInt(left.Text);
            }

            if (left.TokenType == TokenType.LongLiteral)
            {
                return TokenValue.FromLong(left.Text);
            }

            if (left.TokenType == TokenType.StringLiteral)
            {
                return TokenValue.FromString(left.Text);
            }

            if (data.TryGetValue(left.Text, out EntityProperty value))
                return new TokenValue(value);
            return new TokenValue();

        }
    }
}