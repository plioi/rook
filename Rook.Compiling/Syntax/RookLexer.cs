using Parsley;

namespace Rook.Compiling.Syntax
{
    public sealed class RookLexer : Lexer
    {
        public RookLexer(string source)
            : base(new Text(source), new TokenMatcher(TokenKind.IntralineWhiteSpace, @"[ \t]+"),
                         new TokenMatcher(TokenKind.Integer, @"[0-9]+"),
                         new TokenMatcher(TokenKind.Keyword, Keyword),
                         new TokenMatcher(TokenKind.Identifier, @"[a-zA-Z]+[a-zA-Z0-9]*"),
                         new TokenMatcher(TokenKind.Operator, Operator),
                         new TokenMatcher(TokenKind.EndOfLine, @"(\r\n|;)\s*")) { }

        private const string Keyword =
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

        private const string Operator =
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