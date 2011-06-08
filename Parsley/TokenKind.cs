namespace Parsley
{
    public sealed class TokenKind
    {
        private readonly string name;
        private readonly string pattern;

        public TokenKind(string name, string pattern)
        {
            this.name = name;
            this.pattern = pattern;
        }

        public bool TryMatch(Text text, out Token token)
        {
            var match = text.Match(pattern);

            if (match.Success)
            {
                token = new Token(this, text.Position, match.Value);
                return true;
            }

            token = null;
            return false;
        }

        public override string ToString()
        {
            return name + ": " + pattern;
        }
    }
}