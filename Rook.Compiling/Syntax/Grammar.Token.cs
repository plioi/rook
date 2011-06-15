using System;
using System.Collections.Generic;
using System.Linq;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public partial class Grammar
    {
        public static Parser<Token> EndOfLine
        {
            get
            {
                return OnError(GreedyChoice(Token(RookLexer.EndOfLine),
                                      Token(Lexer.EndOfInput)), "end of line");
            }
        }

        public static Parser<Token> Integer
        {
            get { return Token(RookLexer.Integer); }
        }

        public static Parser<Token> Boolean
        {
            get { return Keyword("true", "false"); }
        }

        public static Parser<Token> AnyOperator
        {
            get { return Token(RookLexer.Operator); }
        }

        public static Parser<Token> Operator(params string[] expectedOperators)
        {
            return OnError(Expect(AnyOperator, IsOneOf(expectedOperators)), System.String.Join(", ", expectedOperators));
        }

        public static Parser<Token> AnyKeyword
        {
            get { return Token(RookLexer.Keyword); }
        }

        public static Parser<Token> Keyword(params string[] expectedKeywords)
        {
            return Expect(AnyKeyword, IsOneOf(expectedKeywords));
        }

        public static Parser<Token> Identifier
        {
            get { return Token(RookLexer.Identifier); }
        }

        private static Parser<Token> Token(TokenKind kind)
        {
            return from _ in Optional(Kind(RookLexer.IntralineWhiteSpace))
                   from token in Kind(kind)
                   select token;
        }

        private static Predicate<Token> IsOneOf(IEnumerable<string> values)
        {
            return x => values.Contains(x.Literal);
        }
    }
}