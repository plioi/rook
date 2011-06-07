using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public class Lexer
    {
        private static readonly TokenMatcher UnknownMatcher = new TokenMatcher(TokenKind.Unknown, ".*");
        private static readonly TokenMatcher EndOfInputMatcher = new TokenMatcher(TokenKind.EndOfInput, "$");

        private readonly Text text;
        private readonly IEnumerable<TokenMatcher> matchers;

        public Lexer(Text text, params TokenMatcher[] matchers)
            : this(text, matchers.Concat(new[] { EndOfInputMatcher, UnknownMatcher })) { }

        private Lexer(Text text, IEnumerable<TokenMatcher> matchers)
        {
            this.text = text;
            this.matchers = matchers;
        }

        public Token CurrentToken
        {
            get
            {
                Token token;
                foreach (var matcher in matchers)
                    if (matcher.TryMatch(text, out token))
                        return token;

                //Because of the catch-all Unknown matcher, this should be unreachable.
                return null;
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
        public Position Position { get { return CurrentToken.Position; } }
        public bool EndOfInput { get { return text.EndOfInput; } }
        public override string ToString()
        {
            return text.ToString();
        }
    }
}