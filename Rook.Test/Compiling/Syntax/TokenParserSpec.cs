using Parsley;
using NUnit.Framework;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class TokenParserSpec
    {
        [Test]
        public void ParsesIntegerValues()
        {
            Grammar.Integer.Parses("0").IntoToken(RookLexer.Integer, "0");
            Grammar.Integer.Parses("0 \t ").IntoToken(RookLexer.Integer, "0");

            //NOTE: Rook integer literals are not (yet) limited to int.MaxValue:
            Grammar.Integer.Parses("2147483648").IntoToken(RookLexer.Integer, "2147483648");
        }

        [Test]
        public void ParsesKeywords()
        {
            Grammar.Boolean.Parses("false").IntoToken(RookLexer.Boolean, "false");
            Grammar.Boolean.Parses("false \t ").IntoToken(RookLexer.Boolean, "false");
            Grammar.Boolean.FailsToParse("falsex", "falsex");

            Grammar.Boolean.Parses("true").IntoToken(RookLexer.Boolean, "true");
            Grammar.Boolean.Parses("true \t ").IntoToken(RookLexer.Boolean, "true");
            Grammar.Boolean.FailsToParse("truex", "truex");

            Grammar.@int.Parses("int").IntoToken(RookLexer.@int, "int");
            Grammar.@int.Parses("int \t ").IntoToken(RookLexer.@int, "int");
            Grammar.@int.FailsToParse("intx", "intx");

            Grammar.@bool.Parses("bool").IntoToken(RookLexer.@bool, "bool");
            Grammar.@bool.Parses("bool \t ").IntoToken(RookLexer.@bool, "bool");
            Grammar.@bool.FailsToParse("boolx", "boolx");

            Grammar.@void.Parses("void").IntoToken(RookLexer.@void, "void");
            Grammar.@void.Parses("void \t ").IntoToken(RookLexer.@void, "void");
            Grammar.@void.FailsToParse("voidx", "voidx");

            Grammar.@null.Parses("null").IntoToken(RookLexer.@null, "null");
            Grammar.@null.Parses("null \t ").IntoToken(RookLexer.@null, "null");
            Grammar.@null.FailsToParse("nullx", "nullx");

            Grammar.@if.Parses("if").IntoToken(RookLexer.@if, "if");
            Grammar.@if.Parses("if \t ").IntoToken(RookLexer.@if, "if");
            Grammar.@if.FailsToParse("ifx", "ifx");

            Grammar.@return.Parses("return").IntoToken(RookLexer.@return, "return");
            Grammar.@return.Parses("return \t ").IntoToken(RookLexer.@return, "return");
            Grammar.@return.FailsToParse("returnx", "returnx");

            Grammar.@else.Parses("else").IntoToken(RookLexer.@else, "else");
            Grammar.@else.Parses("else \t ").IntoToken(RookLexer.@else, "else");
            Grammar.@else.FailsToParse("elsex", "elsex");

            Grammar.@fn.Parses("fn").IntoToken(RookLexer.@fn, "fn");
            Grammar.@fn.Parses("fn \t ").IntoToken(RookLexer.@fn, "fn");
            Grammar.@fn.FailsToParse("fnx", "fnx");
        }

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

            var keywords = new[] {"true", "false", "int", "bool", "void", "null", "if", "return", "else", "fn"};

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