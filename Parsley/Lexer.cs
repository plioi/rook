using System.Collections.Generic;

namespace Parsley
{
    public class Lexer
    {
        private readonly Text text;
        private readonly IEnumerable<TokenMatcher> matchers;

        public Lexer(Text text, params TokenMatcher[] matchers)
            : this(text, (IEnumerable<TokenMatcher>)matchers) { }

        private Lexer(Text text, IEnumerable<TokenMatcher> matchers)
        {
            this.text = text;
            this.matchers = matchers;
        }

        public Token CurrentToken
        {
            get
            {
                if (text.EndOfInput)
                    return new Token(TokenKind.EndOfInput, text.Position, text.ToString());

                Token token;
                foreach (var matcher in matchers)
                    if (matcher.TryMatch(text, out token))
                        return token;

                return new Token(TokenKind.Unknown, text.Position, text.ToString());
            }
        }

        public Lexer Advance()
        {
            if (text.EndOfInput)
                return this;

            return new Lexer(text.Advance(CurrentToken.Literal.Length), matchers);
        }

        //TODO: Deprecated.  Was only made public to ease the introduction of a lexing phase.
        public Text Text { get { return text; } }
    }
}