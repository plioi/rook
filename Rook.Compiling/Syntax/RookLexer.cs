using Parsley;

namespace Rook.Compiling.Syntax
{
    public sealed class RookLexer : Lexer
    {
        public static readonly TokenKind IntralineWhiteSpace = new TokenKind();
        public static readonly TokenKind Integer = new TokenKind();
        public static readonly TokenKind Keyword = new TokenKind();
        public static readonly TokenKind Identifier = new TokenKind();
        public static readonly TokenKind Operator = new TokenKind();
        public static readonly TokenKind EndOfLine = new TokenKind();

        public RookLexer(string source)
            : base(new Text(source), new TokenMatcher(IntralineWhiteSpace, @"[ \t]+"),
                         new TokenMatcher(Integer, @"[0-9]+"),
                         new TokenMatcher(Keyword, KeywordPattern),
                         new TokenMatcher(Identifier, @"[a-zA-Z]+[a-zA-Z0-9]*"),
                         new TokenMatcher(Operator, OperatorPattern),
                         new TokenMatcher(EndOfLine, @"(\r\n|;)\s*")) { }

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