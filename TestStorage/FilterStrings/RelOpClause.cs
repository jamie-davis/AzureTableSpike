using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using TestStorage.FilterStrings.LexicalAnalysis;

namespace TestStorage.FilterStrings
{
    internal class RelOpClause : FilterClause
    {
        public FilterClause Lhs { get; }
        public Token Op { get; }
        public FilterClause Rhs { get; }

        public RelOpClause(FilterClause lhs, Token op, FilterClause rhs)
        {
            Lhs = lhs;
            Op = op;
            Rhs = rhs;
        }

        #region Overrides of FilterClause

        public override string Describe()
        {
            return $"{Lhs.Describe()} {Op.Text} {Rhs.Describe()}";
        }

        public override bool Execute(IDictionary<string, EntityProperty> data, out string error)
        {
            var leftResult = Lhs.Execute(data, out error);
            if (error != null)
                return false;

            if (Op.TokenType == TokenType.LogicalOr)
            {
                if (leftResult)
                    return true;

                return Rhs.Execute(data, out error);
            }

            if (Op.TokenType == TokenType.LogicalAnd)
            {
                if (!leftResult)
                    return false;

                return Rhs.Execute(data, out error);
            }

            error = $@"Invalid relational token ""{Op.Text}"".";
            return false;
        }

        #endregion
    }
}