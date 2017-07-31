using System.Collections.Generic;

namespace TestStorage.FilterStrings.LexicalAnalysis
{
    public class Token
    {
        private static readonly List<TokenType> _relops = new List<TokenType>
        {
            TokenType.LogicalAnd, TokenType.LogicalOr
        };

        private static readonly List<TokenType> _operators = new List<TokenType>
        {
            TokenType.CompEq,
            TokenType.CompGe,
            TokenType.CompGt,
            TokenType.CompLe,
            TokenType.CompLt,
            TokenType.CompNe,
        };

        private static readonly List<TokenType> _literals = new List<TokenType>
        {
            TokenType.DateTimeLiteral,
            TokenType.GuidLiteral,
            TokenType.DoubleLiteral,
            TokenType.IntLiteral,
            TokenType.LongLiteral,
            TokenType.StringLiteral,
        };

        public string Text { get; set; }
        public TokenType TokenType { get; set; }

        public Token(TokenType tokenType, string text)
        {
            TokenType = tokenType;
            Text = text;
        }

        public bool IsRelop()
        {
            return _relops.Contains(TokenType);
        }

        private bool IsLiteral()
        {
            return _literals.Contains(TokenType);
        }

        public bool IsOperator()
        {
            return _operators.Contains(TokenType);
        }

        public bool IsData()
        {
            return TokenType == TokenType.Identifier || IsLiteral() || IsRelop() || IsOperator();
        }
    }
}