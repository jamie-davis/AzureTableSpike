using System;

namespace TestStorage.DataAccess
{
    public class InvalidFilterString : Exception
    {
        public InvalidFilterString(string error) : base(error)
        {
        }
    }
}