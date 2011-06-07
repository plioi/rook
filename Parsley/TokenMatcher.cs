namespace Parsley
{
    public sealed class TokenMatcher
    {
        private readonly TokenKind kind;
        private readonly string pattern;

        public TokenMatcher(TokenKind kind, string pattern)
        {
            this.kind = kind;
            this.pattern = pattern;
        }

        public bool TryMatch(Text text, out Token token)
        {
            var match = text.Match(pattern);

            if (match.Success)
            {
                token = new Token(kind, text.Position, match.Value);
                return true;
            }

            token = null;
            return false;
        }
    }
}