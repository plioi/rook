using Parsley;

namespace Rook.Compiling.Syntax
{
    public partial class RookGrammar
    {
        public static Parser<Token> EndOfLine
        {
            get { return Label(Choice(Token(RookLexer.EndOfLine), Token(Lexer.EndOfInput)), "end of line"); }
        }

        public static Parser<Token> Identifier
        {
            get { return Token(RookLexer.Identifier); }
        }
    }
}