namespace Parsley
{
    public sealed class CharLexer : Lexer
    {
        public CharLexer(string source)
            : base(new Text(source), new TokenKind(@".")) { }
    }
}