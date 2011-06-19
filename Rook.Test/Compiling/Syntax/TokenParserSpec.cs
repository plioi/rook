using Parsley;
using NUnit.Framework;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class TokenParserSpec
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
                Grammar.Operator(o).Parses(o).IntoToken(RookLexer.Operators[o], o);
                Grammar.Operator(o).Parses(o + " \t ").IntoToken(RookLexer.Operators[o], o);
                Grammar.Operator(o).FailsToParse("x", "x").WithMessage("(1, 1): " + o + " expected");
            }
        }

        [Test]
        public void ParsesIdentifiers()
        {
            Grammar.Identifier.Parses("a").IntoToken(RookLexer.Identifier, "a");
            Grammar.Identifier.Parses("a \t ").IntoToken(RookLexer.Identifier, "a");
            Grammar.Identifier.Parses("ab").IntoToken(RookLexer.Identifier, "ab");
            Grammar.Identifier.Parses("a0").IntoToken(RookLexer.Identifier, "a0");
            Grammar.Identifier.Parses("a01").IntoToken(RookLexer.Identifier, "a01");

            var keywords = new[] {"true", "false", "int", "bool", "void", "null", "if", "else", "fn"};

            Grammar.Identifier.FailsToParse("0", "0");
            foreach (string keyword in keywords)
                Grammar.Identifier.FailsToParse(keyword, keyword);
        }

        [Test]
        public void ParsesLineEndings()
        {
            //Endlines are the end of input, \r\n, or semicolons (with optional trailing whitspace).

            Grammar.EndOfLine.Parses("").IntoToken(Lexer.EndOfInput, "");

            Grammar.EndOfLine.Parses("\r\n").IntoToken(RookLexer.EndOfLine, "\r\n");
            Grammar.EndOfLine.Parses("\r\n \t \t").IntoToken(RookLexer.EndOfLine, "\r\n \t \t");
            Grammar.EndOfLine.Parses("\r\n \r\n \t ").IntoToken(RookLexer.EndOfLine, "\r\n \r\n \t ");

            Grammar.EndOfLine.Parses(";").IntoToken(RookLexer.EndOfLine, ";");
            Grammar.EndOfLine.Parses("; \t \t").IntoToken(RookLexer.EndOfLine, "; \t \t");
            Grammar.EndOfLine.Parses("; \r\n \t ").IntoToken(RookLexer.EndOfLine, "; \r\n \t ");

            Grammar.EndOfLine.FailsToParse("x", "x").WithMessage("(1, 1): end of line expected");
        }
    }
}