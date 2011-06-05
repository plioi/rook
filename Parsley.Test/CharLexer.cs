namespace Parsley
{
    public sealed class CharLexer : Lexer
    {
        public CharLexer(string source)
            : this(new Text(source)) { }

        public CharLexer(Text text)
            : base(text, new TokenMatcher(typeof(char), @".")) { }
    }
}