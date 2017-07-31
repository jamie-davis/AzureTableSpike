using System.Collections.Generic;
using TestStorage.FilterStrings.LexicalAnalysis;

namespace TestStorage.FilterStrings
{
    internal class TokenKeeper
    {
        private List<Token> _tokens;
        private bool _finished;
        private int _index;
        private static readonly Token _terminator = new Token(TokenType.Terminator, null);

        public TokenKeeper(List<Token> tokens)
        {
            _tokens = tokens;
            _index = 0;
        }

        public TokenKeeper(TokenKeeper tokenKeeper)
        {
            _tokens = tokenKeeper._tokens;
            _index = tokenKeeper._index;
        }

        public bool Finished
        {
            get { return _index >= _tokens.Count; }
        }

        public Token Next => Finished ? _terminator : _tokens[_index];

        public Token Take()
        {
            var next = Next;
            if (next.TokenType != TokenType.Terminator)
                ++_index;
            return next;
        }

        public void Swap(TokenKeeper tokenKeeper)
        {
            var oldTokens = _tokens;
            _tokens = tokenKeeper._tokens;

            var oldIndex = _index;
            _index = tokenKeeper._index;

            tokenKeeper._tokens = oldTokens;
            tokenKeeper._index = oldIndex;
        }
    }
}