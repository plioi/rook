using Parsley;

namespace Rook.Compiling.Syntax
{
    public class RookLexer : Lexer
    {
        public static readonly TokenKind IntralineWhiteSpace = new TokenKind("IntralineWhiteSpace", @"[ \t]+");
        
        public static readonly TokenKind @int = new TokenKind("int", @"int \b");
        public static readonly TokenKind @bool = new TokenKind("bool", @"bool \b");
        public static readonly TokenKind @void = new TokenKind("void", @"void \b");
        public static readonly TokenKind @null = new TokenKind("null", @"null \b");
        public static readonly TokenKind @if = new TokenKind("if", @"if \b");
        public static readonly TokenKind @return = new TokenKind("return", @"return \b");
        public static readonly TokenKind @else = new TokenKind("else", @"else \b");
        public static readonly TokenKind @fn = new TokenKind("fn", @"fn \b");

        public static readonly TokenKind Boolean = new TokenKind("Boolean", @"true \b | false \b");
        public static readonly TokenKind Integer = new TokenKind("Integer", @"[0-9]+");

        public static readonly TokenKind LeftParenthesis = new TokenKind("(", @"\(");
        public static readonly TokenKind RightParenthesis = new TokenKind(")", @"\)");
        public static readonly TokenKind Operator = new TokenKind("Operator", OperatorPattern);

        public static readonly TokenKind Identifier = new TokenKind("Identifier", @"[a-zA-Z]+[a-zA-Z0-9]*");
        public static readonly TokenKind EndOfLine = new TokenKind("EndOfLine", @"(\r\n|;)\s*");

        public RookLexer(string source)
            : base(new Text(source), IntralineWhiteSpace,
            @int, @bool, @void, @null, @if, @return, @else, @fn,
            Boolean, Integer, Identifier,
            LeftParenthesis, RightParenthesis,
            Operator, EndOfLine) { }

        private const string OperatorPattern =
            @"  \*   | /           # Multiplicative
              | \+   | \-          # Additive
              | <=   | <  | >= | > # Relational
              | ==   | !=          # Equality
              | \|\| | && | !      # Logical
              |  =                 # Assignment
              |  ,                 # Comma
              |  {   |  }          # Blocks
              | \[\] | \[|\] | :   # Vectors
              | \?\? | \?          # Nullability";
    }
}