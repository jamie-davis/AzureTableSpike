using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestStorage.FilterStrings
{
    internal class ErrorClause : FilterClause
    {
        public ErrorClause(string error)
        {
            Error = error;
        }

        public string Error { get; }

        #region Overrides of FilterClause

        public override string Describe()
        {
            return $"Error: {Error}";
        }

        public override bool Execute(IDictionary<string, EntityProperty> data, out string error)
        {
            error = Error;
            return false;
        }

        #endregion
    }
}