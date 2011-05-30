using System;
using System.Collections.Generic;
using System.Linq;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public sealed partial class Grammar
    {
        private delegate Parser<Token> TokenParser(Position position);

        private static readonly string[] keywords = new[]
        {
            "true", "false", "int", "bool", "void", "null", 
            "if", "return", "else", "fn"
        };

        private const string operators =
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

        private static readonly string startsWithKeyword = string.Join("|", keywords.Select(k => k + @"\b"));

        public static Parser<Token> EndOfLine
        {
            get
            {
                return OnError(Token(position =>
                                     from line in Choice(String(System.Environment.NewLine, ";"), EndOfInput)
                                     from post in Pattern(@"\s*")
                                     select new Token(TokenKind.EndOfLine, position, line)), "end of line");
            }
        }

        public static Parser<Token> Integer
        {
            get
            {
                return Token(position =>
                             from digits in Pattern(@"[0-9]+")
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
                             from symbol in Pattern(operators)
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
                             from keyword in Pattern(startsWithKeyword)
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
                                    from identifier in Pattern(@"[a-zA-Z]+[a-zA-Z0-9]*")
                                    select new Token(TokenKind.Identifier, position, identifier)),
                              IsNotOneOf(keywords));
            }
        }

        private static Parser<Token> Token(TokenParser goal)
        {
            return from spaces in Pattern(@"[ \t]*")
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