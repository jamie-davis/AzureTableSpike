using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestStorage.FilterStrings
{
    public abstract class FilterClause
    {
        public abstract string Describe();

        #region Overrides of Object

        public override string ToString()
        {
            return Describe();
        }

        #endregion

        public abstract bool Execute(IDictionary<string, EntityProperty> data, out string error);
    }
}