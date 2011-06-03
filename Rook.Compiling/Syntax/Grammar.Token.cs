using System;
using System.Collections.Generic;
using System.Linq;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public sealed partial class Grammar
    {
        private delegate Parser<Token> TokenParser(Position position);

        public static Parser<Token> EndOfLine
        {
            get
            {
                return OnError(Token(position =>
                                     from line in Choice(Pattern(RookLexer.EndOfLinePattern), EndOfInput)
                                     select new Token(TokenKind.EndOfLine, position, line)), "end of line");
            }
        }

        public static Parser<Token> Integer
        {
            get
            {
                return Token(position =>
                             from digits in Pattern(RookLexer.IntegerPattern)
                             select new Token(TokenKind.Integer, position, digits));
            }
        }

        public static Parser<Token> Boolean
        {
            get
            {
                return Keyword("true", "false");
            }
        }

        public static Parser<Token> AnyOperator
        {
            get
            {
                return Token(position =>
                             from symbol in Pattern(RookLexer.OperatorPattern)
                             select new Token(TokenKind.Operator, position, symbol));
            }
        }

        public static Parser<Token> Operator(params string[] expectedOperators)
        {
            return OnError(Expect(AnyOperator, IsOneOf(expectedOperators)), System.String.Join(", ", expectedOperators));
        }

        public static Parser<Token> AnyKeyword
        {
            get
            {
                return Token(position =>
                             from keyword in Pattern(RookLexer.KeywordPattern)
                             select new Token(TokenKind.Keyword, position, keyword));
            }
        }

        public static Parser<Token> Keyword(params string[] expectedKeywords)
        {
            return Expect(AnyKeyword, IsOneOf(expectedKeywords));
        }

        public static Parser<Token> Identifier
        {
            get
            {
                return Expect(Token(position =>
                                    from identifier in Pattern(RookLexer.IdentifierPattern)
                                    select new Token(TokenKind.Identifier, position, identifier)),
                              IsNotOneOf(RookLexer.Keywords));
            }
        }

        private static Parser<Token> Token(TokenParser goal)
        {
            return from spaces in Optional(Pattern(RookLexer.IntralineWhiteSpace))
                   from position in Position
                   from g in goal(position)
                   select g;
        }

        private static Predicate<Token> IsOneOf(IEnumerable<string> values)
        {
            return x => values.Contains(x.Literal);
        }

        private static Predicate<Token> IsNotOneOf(IEnumerable<string> values)
        {
            return x => !values.Contains(x.Literal);
        }
    }
}