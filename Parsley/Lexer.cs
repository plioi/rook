using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public class Lexer : IEnumerable<Token>
    {
        public static readonly TokenKind EndOfInput = new TokenKind("EndOfInput", @"$");
        public static readonly TokenKind Unknown = new TokenKind("Unknown", @".*");

        private readonly Text text;
        private readonly IEnumerable<TokenKind> kinds;

        public Lexer(Text text, params TokenKind[] kinds)
            : this(text, kinds.Concat(new[] { EndOfInput, Unknown })) { }

        private Lexer(Text text, IEnumerable<TokenKind> kinds)
        {
            this.text = text;
            this.kinds = kinds;
        }

        public Token CurrentToken
        {
            get
            {
                Token token;
                foreach (var kind in kinds)
                    if (kind.TryMatch(text, out token))
                        return token;

                return null; //EndOfInput and Unknown guarantee this is unreachable.
            }
        }

        public Lexer Advance()
        {
            if (text.EndOfInput)
                return this;

            return new Lexer(text.Advance(CurrentToken.Literal.Length), kinds);
        }

        public Position Position { get { return text.Position; } }

        public override string ToString()
        {
            return text.ToString();
        }

        public IEnumerator<Token> GetEnumerator()
        {
            var current = CurrentToken;

            yield return current;

            if (current.Kind != EndOfInput)
                foreach (var token in Advance())
                    yield return token;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}