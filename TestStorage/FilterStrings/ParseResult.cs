using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestStorage.FilterStrings
{
    public class ParseResult
    {
        private FilterClause _root;
        public string Error { get; private set; }
        public bool Success => Error == null;

        public FilterClause Root
        {
            get { return _root; }
            set { _root = value; }
        }

        public ParseResult(string error)
        {
            FailedParse(error);
        }

        public ParseResult()
        {
        }

        public void FailedParse(string error)
        {
            Error = error;
        }

        #region Overrides of Object

        public override string ToString()
        {
            return Error ?? Root.Describe();
        }

        #endregion

    }
}