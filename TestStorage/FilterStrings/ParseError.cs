using System;

namespace TestStorage.FilterStrings
{
    public class ParseError : Exception
    {
        public ParseError(string error) : base((string) $"Parse error: {error}")
        {
            
        }
    }
}