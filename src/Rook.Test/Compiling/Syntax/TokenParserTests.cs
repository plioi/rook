using System;
using Parsley;
using Should;

namespace Rook.Compiling.Syntax
{
    [Facts]
    public class TokenParserTests
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

        private readonly RookGrammar grammar = new RookGrammar();

        public void ParsesExpectedOperators()
        {
            var operators = new[]
            {
                "(", ")", "*", "/", "+", "-", "<=", "<", ">=", ">", "!=", "==", "=",
                "&&", "||", "!", ",", "{", "}", "[]", ":", "[", "]", "??", "?", "."
            };

            foreach (var o in operators)
            {
                Grammar.Token(o).Parses(o).WithValue(Operator(o));
                Grammar.Token(o).Parses(o + " \t ").WithValue(Operator(o));
                Grammar.Token(o).FailsToParse("x").LeavingUnparsedTokens("x").WithMessage("(1, 1): " + o + " expected");
            }
        }

        public void ParsesIdentifiers()
        {
            var identifier = grammar.Identifier;

            identifier.Parses("a").WithValue(Token(RookLexer.Identifier, "a"));
            identifier.Parses("a \t ").WithValue(Token(RookLexer.Identifier, "a"));
            identifier.Parses("ab").WithValue(Token(RookLexer.Identifier, "ab"));
            identifier.Parses("a0").WithValue(Token(RookLexer.Identifier, "a0"));
            identifier.Parses("a01").WithValue(Token(RookLexer.Identifier, "a01"));

            var keywords = new[] {"true", "false", "int", "bool", "string", "void", "null", "if", "else", "fn", "class", "new"};

            identifier.FailsToParse("0").LeavingUnparsedTokens("0");
            foreach (string keyword in keywords)
                identifier.FailsToParse(keyword).LeavingUnparsedTokens(keyword);
        }

        public void ParsesLineEndings()
        {
            //Endlines are the end of input, \n, or semicolons (with optional trailing whitespace).
            //Note that Parsley normalizes \r, \n, and \r\n to a single line feed \n.

            var endOfLine = grammar.EndOfLine;

            endOfLine.Parses("").WithValue(Token(TokenKind.EndOfInput, ""));

            endOfLine.Parses("\r\n").WithValue(Token(RookLexer.EndOfLine, "\n"));
            endOfLine.Parses("\r\n \t \t").WithValue(Token(RookLexer.EndOfLine, "\n \t \t"));
            endOfLine.Parses("\r\n \r\n \t ").WithValue(Token(RookLexer.EndOfLine, "\n \n \t "));

            endOfLine.Parses(";").WithValue(Token(RookLexer.EndOfLine, ";"));
            endOfLine.Parses("; \t \t").WithValue(Token(RookLexer.EndOfLine, "; \t \t"));
            endOfLine.Parses("; \r\n \t ").WithValue(Token(RookLexer.EndOfLine, "; \n \t "));

            endOfLine.FailsToParse("x").LeavingUnparsedTokens("x").WithMessage("(1, 1): end of line expected");
        }
    }
}