using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TestStorage.FilterStrings.LexicalAnalysis
{
    public static class LexicalAnalyser
    {
        private static Dictionary<string, Token> _operators = new Dictionary<string, Token>
        {
            {"eq", new Token(TokenType.CompEq, "eq")},
            {"gt", new Token(TokenType.CompGt,"gt")},
            {"ge", new Token(TokenType.CompGe,"ge")},
            {"lt", new Token(TokenType.CompLt,"lt")},
            {"le", new Token(TokenType.CompLe,"le")},
            {"ne", new Token(TokenType.CompNe,"ne")},
            {"and", new Token(TokenType.LogicalAnd,"and")},
            {"not", new Token(TokenType.LogicalNot,"not")},
            {"or", new Token(TokenType.LogicalOr,"or")},
        };

        private static Dictionary<string, Token> _parens = new Dictionary<string, Token>
        {
            {"(", new Token(TokenType.OpenParen, "(")},
            {")", new Token(TokenType.CloseParen, ")")},
        };

        public static IEnumerable<Token> Analyse(string text)
        {
            var stringKeeper = new StringKeeper(text);
            while (!stringKeeper.IsEmpty)
            {
                StringKeeper newStringKeeper;
                Token token;
                if (stringKeeper.IsWhitespace)
                {
                    stringKeeper = stringKeeper.Take();
                }
                else if (TryTakeParens(stringKeeper, out newStringKeeper, out token))
                {
                    stringKeeper = newStringKeeper;
                    yield return token;
                }
                else if (TryTakeOperator(stringKeeper, out newStringKeeper, out token))
                {
                    stringKeeper = newStringKeeper;
                    yield return token;
                }
                else if (TryTakeLiteral(stringKeeper, out newStringKeeper, out token))
                {
                    stringKeeper = newStringKeeper;
                    yield return token;
                }
                else if (TryTakeIdentifier(stringKeeper, out newStringKeeper, out token))
                {
                    stringKeeper = newStringKeeper;
                    yield return token;
                }

            }
        }

        private static bool TryTakeOperator(StringKeeper stringKeeper, out StringKeeper newStringKeeper, out Token token)
        {
            var thisOp = _operators.FirstOrDefault(x => stringKeeper.IsNext(x.Key));
            if (thisOp.Key != null)
            {
                var work = stringKeeper.Take(thisOp.Key.Length);
                if (work.IsEmpty || work.IsWhitespace || work.IsNext("("))
                {
                    newStringKeeper = work;
                    token = thisOp.Value;
                    return true;
                }
            }
            newStringKeeper = stringKeeper;
            token = null;
            return false;
        }

        private static bool TryTakeIdentifier(StringKeeper stringKeeper, out StringKeeper newStringKeeper, out Token token)
        {
            var work = new StringKeeper(stringKeeper);
            if (CanStartIdentifier(work.Next))
            {
                work = work.Take();
                while (ValidIdentifierChar(work.Next))
                {
                    work = work.Take();
                }

                if (work.IsEmpty || work.IsWhitespace)
                {
                    var difference = work.Difference(stringKeeper);
                    newStringKeeper = work;
                    token = new Token(TokenType.Identifier, difference);
                    return true;
                }
            }

            newStringKeeper = stringKeeper;
            token = null;
            return false;
        }

        private static bool TryTakeLiteral(StringKeeper stringKeeper, out StringKeeper newStringKeeper, out Token token)
        {
            var work = new StringKeeper(stringKeeper);

            if (TryTakeStringLiteral(work, out newStringKeeper, out token))
                return true;

            if (TryTakeDateTime(work, out newStringKeeper, out token))
                return true;

            if (TryTakeGuid(work, out newStringKeeper, out token))
                return true;

            if (TryTakeNumeric(work, out newStringKeeper, out token))
                return true;

            newStringKeeper = stringKeeper;
            token = null;
            return false;
        }

        private static bool TryTakeGuid(StringKeeper stringKeeper, out StringKeeper newStringKeeper, out Token token)
        {
            if (stringKeeper.IsNext("guid'"))
            {
                var work = stringKeeper.Take(4);
                Debug.Assert(work.Next == '\'');

                StringKeeper literalEnd;
                Token literalToken;
                TryTakeStringLiteral(work, out literalEnd, out literalToken);
                newStringKeeper = literalEnd;

                if (literalToken.TokenType == TokenType.Error)
                {
                    token = new Token(TokenType.Error, literalEnd.Difference(stringKeeper));
                    return true;
                }

                literalToken.TokenType = TokenType.GuidLiteral;
                token = literalToken;
                return true;
            }

            newStringKeeper = stringKeeper;
            token = null;
            return false;
        }

        private static bool TryTakeParens(StringKeeper stringKeeper, out StringKeeper newStringKeeper, out Token token)
        {
            var thisParens = _parens.FirstOrDefault(x => stringKeeper.IsNext(x.Key));
            if (thisParens.Key != null)
            {
                var work = stringKeeper.Take(thisParens.Key.Length);
                newStringKeeper = work;
                token = thisParens.Value;
                return true;
            }

            newStringKeeper = stringKeeper;
            token = null;
            return false;
        }

        private static bool TryTakeDateTime(StringKeeper stringKeeper, out StringKeeper newStringKeeper, out Token token)
        {
            if (stringKeeper.IsNext("datetime'"))
            {
                var work = stringKeeper.Take(8);
                Debug.Assert(work.Next == '\'');

                StringKeeper literalEnd;
                Token literalToken;
                TryTakeStringLiteral(work, out literalEnd, out literalToken);
                newStringKeeper = literalEnd;

                if (literalToken.TokenType == TokenType.Error)
                {
                    token = new Token(TokenType.Error, literalEnd.Difference(stringKeeper));
                    return true;
                }

                literalToken.TokenType = TokenType.DateTimeLiteral;
                token = literalToken;
                return true;
            }

            newStringKeeper = stringKeeper;
            token = null;
            return false;
        }

        private static bool TryTakeNumeric(StringKeeper stringKeeper, out StringKeeper newStringKeeper, out Token token)
        {
            if (char.IsDigit(stringKeeper.Next))
            {
                var invalidNumber = false;
                var work = new StringKeeper(stringKeeper);
                var doubleValue = false;
                do
                {
                    work = work.Take();
                    if (!work.IsEmpty && work.Next == '.' && !doubleValue)
                    {
                        doubleValue = true;
                        work = work.Take();

                        if (!char.IsDigit(work.Next))
                            invalidNumber = true;
                    }
                } while (!work.IsEmpty && char.IsDigit(work.Next));

                var value = work.Difference(stringKeeper);
                var longInt = false;
                if (!work.IsEmpty && work.Next == 'L')
                {
                    longInt = true;
                    work = work.Take();
                }

                if (!IsTerminator(work))
                {
                    do
                    {
                        work = work.Take();
                    } while (!work.IsWhitespace && !work.IsEmpty);

                    token = new Token(TokenType.Error, work.Difference(stringKeeper));
                    newStringKeeper = work;
                    return true;
                }

                TokenType tokenType;
                if (invalidNumber || (longInt && doubleValue))
                    tokenType = TokenType.Error;
                else
                {
                    if (doubleValue)
                        tokenType = TokenType.DoubleLiteral;
                    else
                        tokenType = longInt ? TokenType.LongLiteral : TokenType.IntLiteral;
                }

                token = new Token(tokenType, value);
                newStringKeeper = work;
                return true;
            }

            newStringKeeper = stringKeeper;
            token = null;
            return false;
        }

        private static bool TryTakeStringLiteral(StringKeeper stringKeeper, out StringKeeper newStringKeeper, out Token token)
        {
            if (stringKeeper.Next == '\'')
            {
                var start = stringKeeper.Take();
                var work = new StringKeeper(start);
                while (!work.IsEmpty)
                {
                    if (work.IsNext("\'\'"))
                    {
                        work = work.Take(2);
                    }
                    else
                    {
                        if (work.Next == '\'')
                        {
                            var literalValue = work.Difference(start).Replace("''", "'");

                            token = new Token(TokenType.StringLiteral, literalValue);
                            work = work.Take();
                            newStringKeeper = work;

                            if (!IsTerminator(work))
                            {
                                do
                                {
                                    work = work.Take();
                                } while (!work.IsEmpty && !work.IsWhitespace);

                                token.TokenType = TokenType.Error;
                                return true;
                            }

                            return true;
                        }
                        else
                            work = work.Take();
                    }
                }

                newStringKeeper = work;
                token = new Token(TokenType.Error, work.Difference(stringKeeper));
                return true;
            }

            newStringKeeper = stringKeeper;
            token = null;
            return false;
        }

        private static bool IsTerminator(StringKeeper stringKeeper)
        {
            return stringKeeper.IsEmpty || stringKeeper.IsWhitespace || _parens.ContainsKey(stringKeeper.Next.ToString());
        }

        private static bool ValidIdentifierChar(char next)
        {
            const string validPunctuation = "_";
            return !char.IsWhiteSpace(next) && (char.IsLetter(next) || char.IsNumber(next) || validPunctuation.Contains(next));
        }

        private static bool CanStartIdentifier(char next)
        {
            const string validStarters = "@_";
            return char.IsLetter(next) || validStarters.Contains(next);
        }
    }
}
