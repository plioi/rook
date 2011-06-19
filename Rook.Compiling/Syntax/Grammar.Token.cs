using Parsley;

namespace Rook.Compiling.Syntax
{
    public partial class Grammar
    {
        public static Parser<Token> EndOfLine
        {
            get { return OnError(Choice(Token(RookLexer.EndOfLine), Token(Lexer.EndOfInput)), "end of line"); }
        }

        public static Parser<Token> Operator(string symbol)
        {
            return Token(RookLexer.Operators[symbol]);
        }

        public static Parser<Token> Identifier
        {
            get { return Token(RookLexer.Identifier); }
        }

        private static Parser<Token> Token(TokenKind kind)
        {
            return from token in Kind(kind)
                   from _ in Optional(Kind(RookLexer.IntralineWhiteSpace))
                   select token;
        }
    }
}