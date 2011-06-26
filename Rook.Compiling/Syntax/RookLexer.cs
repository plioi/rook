using Parsley;

namespace Rook.Compiling.Syntax
{
    public class RookLexer : Lexer
    {
        private static readonly TokenKind IntralineWhitespace = new TokenKind("intra-line whitespace", @"[ \t]+", skippable: true);

        public static readonly Keyword @int = new Keyword("int");
        public static readonly Keyword @bool = new Keyword("bool");
        public static readonly Keyword @void = new Keyword("void");
        public static readonly Keyword @null = new Keyword("null");
        public static readonly Keyword @if = new Keyword("if");
        public static readonly Keyword @else = new Keyword("else");
        public static readonly Keyword @fn = new Keyword("fn");
        public static readonly Keyword @true = new Keyword("true");
        public static readonly Keyword @false = new Keyword("false");

        public static readonly TokenKind Integer = new TokenKind("integer", @"[0-9]+");
        public static readonly TokenKind Identifier = new TokenKind("identifier", @"[a-zA-Z]+[a-zA-Z0-9]*");
        public static readonly TokenKind EndOfLine = new TokenKind("end of line", @"(\r\n|;)\s*");

        public RookLexer(string source)
            : base(new Text(source),
            IntralineWhitespace,
            @int, @bool, @void, @null, @if, @else, @fn, @true, @false,
            Integer, Identifier,
            new Operator("("), new Operator(")"),
            new Operator("*"), new Operator("/"),
            new Operator("+"), new Operator("-"),
            new Operator("<="), new Operator("<"), new Operator(">="), new Operator(">"),
            new Operator("=="), new Operator("!="),
            new Operator("||"), new Operator("&&"), new Operator("!"),
            new Operator("="), new Operator(","),
            new Operator("{"), new Operator("}"),
            new Operator("[]"), new Operator("["), new Operator("]"), new Operator(":"),
            new Operator("??"), new Operator("?"),
            EndOfLine) { }
    }
}