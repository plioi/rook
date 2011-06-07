namespace Rook.Compiling.Syntax
{
    public class TokenKind : Parsley.TokenKind
    {
        public static readonly TokenKind IntralineWhiteSpace = new TokenKind();
        public static readonly TokenKind Integer = new TokenKind();
        public static readonly TokenKind Keyword = new TokenKind();
        public static readonly TokenKind Identifier = new TokenKind();
        public static readonly TokenKind Operator = new TokenKind();
        public static readonly TokenKind EndOfLine = new TokenKind();
    }
}