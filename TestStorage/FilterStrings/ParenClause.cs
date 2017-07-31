using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestStorage.FilterStrings
{
    internal class ParenClause : FilterClause
    {
        public FilterClause Clause { get; }

        public ParenClause(FilterClause clause)
        {
            Clause = clause;
        }

        #region Overrides of FilterClause

        public override string Describe()
        {
            return $"({Clause.Describe()})";
        }

        public override bool Execute(IDictionary<string, EntityProperty> data, out string error)
        {
            return Clause.Execute(data, out error);
        }

        #endregion
    }
}