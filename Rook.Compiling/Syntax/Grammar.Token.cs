using System;
using System.Collections.Generic;
using System.Linq;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public sealed partial class Grammar
    {
        public static Parser<Token> EndOfLine
        {
            get
            {
                return OnError(Choice(Token(TokenKind.EndOfLine),
                                      Token(Parsley.TokenKind.EndOfInput)), "end of line");
            }
        }

        public static Parser<Token> Integer
        {
            get { return Token(TokenKind.Integer); }
        }

        public static Parser<Token> Boolean
        {
            get { return Keyword("true", "false"); }
        }

        public static Parser<Token> AnyOperator
        {
            get { return Token(TokenKind.Operator); }
        }

        public static Parser<Token> Operator(params string[] expectedOperators)
        {
            return OnError(Expect(AnyOperator, IsOneOf(expectedOperators)), String.Join(", ", expectedOperators));
        }

        public static Parser<Token> AnyKeyword
        {
            get { return Token(TokenKind.Keyword); }
        }

        public static Parser<Token> Keyword(params string[] expectedKeywords)
        {
            return Expect(AnyKeyword, IsOneOf(expectedKeywords));
        }

        public static Parser<Token> Identifier
        {
            get { return Token(TokenKind.Identifier); }
        }

        private static Parser<Token> Token(object kind)
        {
            return from _ in Optional(Kind(TokenKind.IntralineWhiteSpace))
                   from token in Kind(kind)
                   select token;
        }

        private static Predicate<Token> IsOneOf(IEnumerable<string> values)
        {
            return x => values.Contains(x.Literal);
        }
    }
}