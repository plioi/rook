using Parsley;
using NUnit.Framework;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class TokenParserSpec : Grammar
    {
        [Test]
        public void ParsesExpectedOperators()
        {
            var operators = new[]
            {
                "(", ")", "*", "/", "+", "-", "<=", "<", ">=", ">", "!=", "==", "=",
                "&&", "||", "!", ",", "{", "}", "[]", ":", "[", "]", "??", "?"
            };

            foreach (var o in operators)
            {
                Token(o).Parses(o).IntoToken(o).Value.Kind.ShouldBeInstanceOf<Operator>();
                Token(o).Parses(o + " \t ").IntoToken(o).Value.Kind.ShouldBeInstanceOf<Operator>();
                Token(o).FailsToParse("x", "x").WithMessage("(1, 1): " + o + " expected");
            }
        }

        [Test]
        public void ParsesIdentifiers()
        {
            Identifier.Parses("a").IntoToken(RookLexer.Identifier, "a");
            Identifier.Parses("a \t ").IntoToken(RookLexer.Identifier, "a");
            Identifier.Parses("ab").IntoToken(RookLexer.Identifier, "ab");
            Identifier.Parses("a0").IntoToken(RookLexer.Identifier, "a0");
            Identifier.Parses("a01").IntoToken(RookLexer.Identifier, "a01");

            var keywords = new[] {"true", "false", "int", "bool", "void", "null", "if", "else", "fn"};

            Identifier.FailsToParse("0", "0");
            foreach (string keyword in keywords)
                Identifier.FailsToParse(keyword, keyword);
        }

        [Test]
        public void ParsesLineEndings()
        {
            //Endlines are the end of input, \r\n, or semicolons (with optional trailing whitspace).

            EndOfLine.Parses("").IntoToken(Lexer.EndOfInput, "");

            EndOfLine.Parses("\r\n").IntoToken(RookLexer.EndOfLine, "\r\n");
            EndOfLine.Parses("\r\n \t \t").IntoToken(RookLexer.EndOfLine, "\r\n \t \t");
            EndOfLine.Parses("\r\n \r\n \t ").IntoToken(RookLexer.EndOfLine, "\r\n \r\n \t ");

            EndOfLine.Parses(";").IntoToken(RookLexer.EndOfLine, ";");
            EndOfLine.Parses("; \t \t").IntoToken(RookLexer.EndOfLine, "; \t \t");
            EndOfLine.Parses("; \r\n \t ").IntoToken(RookLexer.EndOfLine, "; \r\n \t ");

            EndOfLine.FailsToParse("x", "x").WithMessage("(1, 1): end of line expected");
        }
    }
}