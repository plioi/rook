using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public sealed class Lexer
    {
        private delegate Token TokenMatcher(Text text);

        private readonly Text text;
        private readonly IEnumerable<TokenMatcher> matchers;

        public Lexer(Text text, params string[] patterns)
            : this(text, patterns.Select(Matcher))
        {
        }

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
                    return null;

                foreach (var match in matchers)
                {
                    var token = match(text);
                    if (token != null)
                        return token;
                }

                string unknownToken = text.ToString();
                return new Token(text.Position, unknownToken);
            }
        }

        public Lexer Advance()
        {
            if (text.EndOfInput)
                return this;

            return new Lexer(text.Advance(CurrentToken.Literal.Length), matchers);
        }

        private static TokenMatcher Matcher(string pattern)
        {
            return text =>
            {
                var match = text.Match(pattern);

                return match.Success ? new Token(text.Position, match.Value) : null;
            };
        }
    }
}