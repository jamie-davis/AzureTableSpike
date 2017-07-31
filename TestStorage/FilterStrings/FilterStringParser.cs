using System.Collections.Generic;
using System.Linq;
using TestStorage.FilterStrings.LexicalAnalysis;

namespace TestStorage.FilterStrings
{
    public static class FilterStringParser
    {
        public static ParseResult Parse(string filter)
        {
            var tokens = LexicalAnalyser.Analyse(filter);
            var firstErr = tokens.FirstOrDefault(t => t.TokenType == TokenType.Error);
            if (firstErr != null)
            {
                var lexingError = $@"""{firstErr.Text}"" is not a valid token.";
                return new ParseResult(lexingError);
            }

            var result = new ParseResult();

            string error;
            result.Root = PerformParse(tokens, out error);
            if (result.Root == null)
                result.FailedParse(error);

            return result;
        }

        private static FilterClause PerformParse(IEnumerable<Token> tokens, out string error)
        {
            var tokenKeeper = new TokenKeeper(tokens.ToList());
            var clause = TakeClause(tokenKeeper);

            if (clause is ErrorClause errorClause)
                error = errorClause.Error;
            else
                error = null;

            return clause;
        }

        private static FilterClause TakeClause(TokenKeeper tokenKeeper)
        {
            FilterClause clause;
            if (TryTakeExpression(tokenKeeper, out clause))
            {
                if (tokenKeeper.Finished || TryTakeRelOp(tokenKeeper, ref clause))
                    return clause;
            }

            return new ErrorClause("Unable to parse filter string.");

        }

        private static bool TryTakeRelOp(TokenKeeper tokenKeeper, ref FilterClause clause)
        {
            if (tokenKeeper.Next.IsRelop())
            {
                var work = new TokenKeeper(tokenKeeper);
                var op = work.Take();

                FilterClause rhs;
                if (TryTakeExpression(work, out rhs))
                {
                    var relOpClause = new RelOpClause(clause, op, rhs);
                    tokenKeeper.Swap(work);
                    clause = relOpClause;
                    return true;
                }
            }

            return false;
        }

        private static bool TryTakeExpression(TokenKeeper tokenKeeper, out FilterClause clause)
        {
            if (TryTakeParenExpression(tokenKeeper, out clause)
                || TryTakeComparison(tokenKeeper, out clause))
            {
                var work = new TokenKeeper(tokenKeeper);
                if (TryTakeRelOp(work, ref clause))
                    tokenKeeper.Swap(work);
                return true;
            }

            clause = null;
            return false;
        }

        private static bool TryTakeParenExpression(TokenKeeper tokenKeeper, out FilterClause clause)
        {
            if (tokenKeeper.Next.TokenType == TokenType.OpenParen)
            {
                var work = new TokenKeeper(tokenKeeper);
                work.Take();
                FilterClause expression;
                if (TryTakeExpression(work, out expression) && work.Next.TokenType == TokenType.CloseParen)
                {
                    work.Take();
                    clause = new ParenClause(expression);
                    tokenKeeper.Swap(work);
                    return true;
                }
            }

            clause = null;
            return false;
        }

        private static bool TryTakeComparison(TokenKeeper tokenKeeper, out FilterClause clause)
        {
            Token left, right, op;
            var work = new TokenKeeper(tokenKeeper);
            if (TryTakeData(work, out left) && TryTakeOperator(work, out op) &&
                TryTakeData(work, out right))
            {
                tokenKeeper.Swap(work);
                clause = new ComparisonClause(left, op, right);
                return true;
            }

            clause = null;
            return false;
        }

        private static bool TryTakeData(TokenKeeper tokenKeeper, out Token data)
        {
            if (tokenKeeper.Next.IsData())
            {
                data = tokenKeeper.Take();
                return true;
            }

            data = new Token(TokenType.Error, null);
            return false;
        }

        private static bool TryTakeOperator(TokenKeeper tokenKeeper, out Token op)
        {
            if (tokenKeeper.Next.IsOperator())
            {
                op = tokenKeeper.Take();
                return true;
            }

            op = new Token(TokenType.Error, null);
            return false;
        }
    }
}
