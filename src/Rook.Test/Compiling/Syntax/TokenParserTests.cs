using System;
using Parsley;
using Should;

namespace Rook.Compiling.Syntax
{
    [Facts]
    public class TokenParserTests : RookGrammar
    {
        private static Action<Token> Token(TokenKind expectedKind, string expectedLiteral)
        {
            return t => t.ShouldEqual(expectedKind, expectedLiteral);
        }

        private static Action<Token> Operator(string expectedLiteral)
        {
            return t =>
            {
                t.Literal.ShouldEqual(expectedLiteral);
                t.Kind.ShouldBeType<Operator>();
            };
        }

        public void ParsesExpectedOperators()
        {
            var operators = new[]
            {
                "(", ")", "*", "/", "+", "-", "<=", "<", ">=", ">", "!=", "==", "=",
                "&&", "||", "!", ",", "{", "}", "[]", ":", "[", "]", "??", "?", "."
            };

            foreach (var o in operators)
            {
                Token(o).Parses(o).WithValue(Operator(o));
                Token(o).Parses(o + " \t ").WithValue(Operator(o));
                Token(o).FailsToParse("x").LeavingUnparsedTokens("x").WithMessage("(1, 1): " + o + " expected");
            }
        }

        public void ParsesIdentifiers()
        {
            Identifier.Parses("a").WithValue(Token(RookLexer.Identifier, "a"));
            Identifier.Parses("a \t ").WithValue(Token(RookLexer.Identifier, "a"));
            Identifier.Parses("ab").WithValue(Token(RookLexer.Identifier, "ab"));
            Identifier.Parses("a0").WithValue(Token(RookLexer.Identifier, "a0"));
            Identifier.Parses("a01").WithValue(Token(RookLexer.Identifier, "a01"));

            var keywords = new[] {"true", "false", "int", "bool", "string", "void", "null", "if", "else", "fn", "class", "new"};

            Identifier.FailsToParse("0").LeavingUnparsedTokens("0");
            foreach (string keyword in keywords)
                Identifier.FailsToParse(keyword).LeavingUnparsedTokens(keyword);
        }

        public void ParsesLineEndings()
        {
            //Endlines are the end of input, \n, or semicolons (with optional trailing whitespace).
            //Note that Parsley normalizes \r, \n, and \r\n to a single line feed \n.

            EndOfLine.Parses("").WithValue(Token(TokenKind.EndOfInput, ""));

            EndOfLine.Parses("\r\n").WithValue(Token(RookLexer.EndOfLine, "\n"));
            EndOfLine.Parses("\r\n \t \t").WithValue(Token(RookLexer.EndOfLine, "\n \t \t"));
            EndOfLine.Parses("\r\n \r\n \t ").WithValue(Token(RookLexer.EndOfLine, "\n \n \t "));

            EndOfLine.Parses(";").WithValue(Token(RookLexer.EndOfLine, ";"));
            EndOfLine.Parses("; \t \t").WithValue(Token(RookLexer.EndOfLine, "; \t \t"));
            EndOfLine.Parses("; \r\n \t ").WithValue(Token(RookLexer.EndOfLine, "; \n \t "));

            EndOfLine.FailsToParse("x").LeavingUnparsedTokens("x").WithMessage("(1, 1): end of line expected");
        }
    }
}