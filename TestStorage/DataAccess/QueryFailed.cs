using System;

namespace TestStorage.DataAccess
{
    public class QueryFailed : Exception
    {
        public QueryFailed(string error) : base(error)
        {
        }
    }
}