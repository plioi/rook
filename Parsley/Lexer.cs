using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public sealed class Lexer
    {
        private delegate string TokenMatcher(Text text);

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

        public string CurrentToken
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
                return unknownToken;
            }
        }

        public Lexer Advance()
        {
            if (text.EndOfInput)
                return this;

            return new Lexer(text.Advance(CurrentToken.Length), matchers);
        }

        private static TokenMatcher Matcher(string pattern)
        {
            return text =>
            {
                var match = text.Match(pattern);

                return match.Success ? match.Value : null;
            };
        }
    }
}