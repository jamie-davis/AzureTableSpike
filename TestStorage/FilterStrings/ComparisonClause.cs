using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Schema;
using TestStorage.FilterStrings.LexicalAnalysis;

namespace TestStorage.FilterStrings
{
    internal class ComparisonClause : FilterClause
    {
        public Token Left { get; }
        public Token Op { get; }
        public Token Right { get; }

        public ComparisonClause(Token left, Token op, Token right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        #region Overrides of FilterClause

        public override string Describe()
        {
            return $"{Left.Text} {Op.Text} {Right.Text}";
        }

        public override bool Execute(IDictionary<string, EntityProperty> data, out string error)
        {
            var left = TokenValueLoader.Load(Left, data);
            var right = TokenValueLoader.Load(Right, data);

            if (!left.IsGood)
            {
                error = $@"Unable to determine value for ""{Left.Text}""";
                return false;
            }

            if (!right.IsGood)
            {
                error = $@"Unable to determine value for ""{Right.Text}""";
                return false;
            }

            return left.Compare(Op, right, out error);
        }

        #endregion
    }
}