using Parsley;

namespace Rook.Compiling.Syntax
{
    public partial class Grammar
    {
        public static Parser<Token> EndOfLine
        {
            get
            {
                return OnError(Choice(Token(RookLexer.EndOfLine),
                                            Token(Lexer.EndOfInput)), "end of line");
            }
        }

        public static Parser<Token> @int { get { return Token(RookLexer.@int); } }
        public static Parser<Token> @bool { get { return Token(RookLexer.@bool); } }
        public static Parser<Token> @void { get { return Token(RookLexer.@void); } }
        public static Parser<Token> @null { get { return Token(RookLexer.@null); } }
        public static Parser<Token> @if { get { return Token(RookLexer.@if); } }
        public static Parser<Token> @return { get { return Token(RookLexer.@return); } }
        public static Parser<Token> @else { get { return Token(RookLexer.@else); } }
        public static Parser<Token> @fn { get { return Token(RookLexer.@fn); } }

        public static Parser<Token> Integer { get { return Token(RookLexer.Integer); } }
        public static Parser<Token> Boolean { get { return Token(RookLexer.Boolean); } }

        public static Parser<Token> Operator(string symbol)
        {
            return OnError(Expect(Token(RookLexer.Operators[symbol]), x => x.Literal == symbol), symbol);
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