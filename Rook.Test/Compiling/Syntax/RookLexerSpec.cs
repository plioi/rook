using System;
using NUnit.Framework;
using Parsley;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class RookLexerSpec : AbstractGrammar
    {
        [Test]
        public void ShouldRecognizeIntralineWhiteSpaces()
        {
            AssertTokens(" ", token => token.ShouldBe(RookLexer.IntralineWhiteSpace, " "));
            AssertTokens("\t", token => token.ShouldBe(RookLexer.IntralineWhiteSpace, "\t"));
            AssertTokens(" \t ", token => token.ShouldBe(RookLexer.IntralineWhiteSpace, " \t "));
            AssertTokens("\t \t", token => token.ShouldBe(RookLexer.IntralineWhiteSpace, "\t \t"));
        }

        [Test]
        public void ShouldRecognizeIntegers()
        {
            AssertTokens("0", token => token.ShouldBe(RookLexer.Integer, "0"));
            
            //NOTE: Integer literals are not (yet) limited to int.MaxValue:
            AssertTokens("2147483648", token => token.ShouldBe(RookLexer.Integer, "2147483648"));
        }

        [Test]
        public void ShouldRecognizeKeywords()
        {
            AssertTokens("true", token => token.ShouldBe(RookLexer.Keyword, "true"));
            AssertTokens("false", token => token.ShouldBe(RookLexer.Keyword, "false"));
            AssertTokens("int", token => token.ShouldBe(RookLexer.Keyword, "int"));
            AssertTokens("bool", token => token.ShouldBe(RookLexer.Keyword, "bool"));
            AssertTokens("void", token => token.ShouldBe(RookLexer.Keyword, "void"));
            AssertTokens("null", token => token.ShouldBe(RookLexer.Keyword, "null"));
            AssertTokens("if", token => token.ShouldBe(RookLexer.Keyword, "if"));
            AssertTokens("return", token => token.ShouldBe(RookLexer.Keyword, "return"));
            AssertTokens("else", token => token.ShouldBe(RookLexer.Keyword, "else"));
            AssertTokens("fn", token => token.ShouldBe(RookLexer.Keyword, "fn"));
        }

        [Test]
        public void ShouldRecognizeIdentifiers()
        {
            AssertTokens("a", token => token.ShouldBe(RookLexer.Identifier, "a"));
            AssertTokens("ab", token => token.ShouldBe(RookLexer.Identifier, "ab"));
            AssertTokens("a0", token => token.ShouldBe(RookLexer.Identifier, "a0"));
        }

        [Test]
        public void ShouldRecognizeOperators()
        {
            AssertTokens("<=>=<>!====*/+-&&||!{}[][,]()???:",
                token => token.ShouldBe(RookLexer.Operator, "<="),
                token => token.ShouldBe(RookLexer.Operator, ">="),
                token => token.ShouldBe(RookLexer.Operator, "<"),
                token => token.ShouldBe(RookLexer.Operator, ">"),
                token => token.ShouldBe(RookLexer.Operator, "!="),
                token => token.ShouldBe(RookLexer.Operator, "=="),
                token => token.ShouldBe(RookLexer.Operator, "="),
                token => token.ShouldBe(RookLexer.Operator, "*"),
                token => token.ShouldBe(RookLexer.Operator, "/"),
                token => token.ShouldBe(RookLexer.Operator, "+"),
                token => token.ShouldBe(RookLexer.Operator, "-"),
                token => token.ShouldBe(RookLexer.Operator, "&&"),
                token => token.ShouldBe(RookLexer.Operator, "||"),
                token => token.ShouldBe(RookLexer.Operator, "!"),
                token => token.ShouldBe(RookLexer.Operator, "{"),
                token => token.ShouldBe(RookLexer.Operator, "}"),
                token => token.ShouldBe(RookLexer.Operator, "[]"),
                token => token.ShouldBe(RookLexer.Operator, "["),
                token => token.ShouldBe(RookLexer.Operator, ","),
                token => token.ShouldBe(RookLexer.Operator, "]"),
                token => token.ShouldBe(RookLexer.Operator, "("),
                token => token.ShouldBe(RookLexer.Operator, ")"),
                token => token.ShouldBe(RookLexer.Operator, "??"),
                token => token.ShouldBe(RookLexer.Operator, "?"),
                token => token.ShouldBe(RookLexer.Operator, ":"));
        }

        [Test]
        public void ShouldRecognizeEndOfLogicalLine()
        {
            //Endlines are \r\n or semicolons (with optional preceding spaces/tabs and optional trailing whitspace).

            AssertTokens("\r\n", token => token.ShouldBe(RookLexer.EndOfLine, "\r\n"));
            AssertTokens("\r\n \r\n \t ", token => token.ShouldBe(RookLexer.EndOfLine, "\r\n \r\n \t "));

            AssertTokens(";", token => token.ShouldBe(RookLexer.EndOfLine, ";"));
            AssertTokens("; \r\n \t ", token => token.ShouldBe(RookLexer.EndOfLine, "; \r\n \t "));
        }

        private static void AssertTokens(string source, params Action<Token>[] assertions)
        {
            Lexer lexer = new RookLexer(source);

            foreach (var assertToken in assertions)
            {
                assertToken(lexer.CurrentToken);
                lexer = lexer.Advance();
            }

            lexer.CurrentToken.Kind.ShouldEqual(Lexer.EndOfInput);
        }
    }
}