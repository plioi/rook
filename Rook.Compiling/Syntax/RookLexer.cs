using Parsley;

namespace Rook.Compiling.Syntax
{
    public sealed class RookLexer : Lexer
    {
        public static readonly TokenKind IntralineWhiteSpace = new TokenKind(@"[ \t]+");
        public static readonly TokenKind Integer = new TokenKind(@"[0-9]+");
        public static readonly TokenKind Keyword = new TokenKind(KeywordPattern);
        public static readonly TokenKind Identifier = new TokenKind(@"[a-zA-Z]+[a-zA-Z0-9]*");
        public static readonly TokenKind Operator = new TokenKind(OperatorPattern);
        public static readonly TokenKind EndOfLine = new TokenKind(@"(\r\n|;)\s*");

        public RookLexer(string source)
            : base(new Text(source), IntralineWhiteSpace, Integer, Keyword, Identifier, Operator, EndOfLine) { }

        private const string KeywordPattern =
            @"  true   \b
              | false  \b
              | int    \b
              | bool   \b
              | void   \b
              | null   \b
              | if     \b
              | return \b
              | else   \b
              | fn     \b";

        private const string OperatorPattern =
            @"  \(   | \)          # Parentheses
              | \*   | /           # Multiplicative
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