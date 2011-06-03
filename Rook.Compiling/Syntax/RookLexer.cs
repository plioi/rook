using System.Linq;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public sealed class RookLexer : Lexer
    {
        public const string IntegerPattern = @"[0-9]+";

        public const string OperatorPattern =
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

        public static readonly string[] Keywords = new[]
        {
            "true", "false", "int", "bool", "void", "null", "if", "return", "else", "fn"
        };

        public static readonly string KeywordPattern = string.Join("|", Keywords.Select(k => k + @"\b"));

        public const string IdentifierPattern = @"[a-zA-Z]+[a-zA-Z0-9]*";

        public const string EndOfLinePattern = @"(\r\n|;)\s*";

        public const string IntralineWhiteSpace = @"[ \t]+";

        public RookLexer(Text text)
            : base(text, new TokenMatcher(TokenKind.IntralineWhiteSpace, IntralineWhiteSpace),
                         new TokenMatcher(TokenKind.Integer, IntegerPattern),
                         new TokenMatcher(TokenKind.Keyword, KeywordPattern),
                         new TokenMatcher(TokenKind.Identifier, IdentifierPattern),
                         new TokenMatcher(TokenKind.Operator, OperatorPattern),
                         new TokenMatcher(TokenKind.EndOfLine, EndOfLinePattern)) { }
    }
}