using Parsley;

namespace Rook.Compiling.Syntax
{
    public class RookLexer : Lexer
    {
        public static readonly TokenKind IntralineWhiteSpace = new TokenKind("IntralineWhiteSpace", @"[ \t]+");
        public static readonly TokenKind Integer = new TokenKind("Integer", @"[0-9]+");
        public static readonly TokenKind Keyword = new TokenKind("Keyword", KeywordPattern);
        public static readonly TokenKind Identifier = new TokenKind("Identifier", @"[a-zA-Z]+[a-zA-Z0-9]*");
        public static readonly TokenKind Operator = new TokenKind("Operator", OperatorPattern);
        public static readonly TokenKind EndOfLine = new TokenKind("EndOfLine", @"(\r\n|;)\s*");

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