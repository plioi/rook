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
            AssertTokens(" ", token => token.ShouldBe(TokenKind.IntralineWhiteSpace, " "));
            AssertTokens("\t", token => token.ShouldBe(TokenKind.IntralineWhiteSpace, "\t"));
            AssertTokens(" \t ", token => token.ShouldBe(TokenKind.IntralineWhiteSpace, " \t "));
            AssertTokens("\t \t", token => token.ShouldBe(TokenKind.IntralineWhiteSpace, "\t \t"));
        }

        [Test]
        public void ShouldRecognizeIntegers()
        {
            AssertTokens("0", token => token.ShouldBe(TokenKind.Integer, "0"));
            
            //NOTE: Integer literals are not (yet) limited to int.MaxValue:
            AssertTokens("2147483648", token => token.ShouldBe(TokenKind.Integer, "2147483648"));
        }

        [Test]
        public void ShouldRecognizeKeywords()
        {
            AssertTokens("true", token => token.ShouldBe(TokenKind.Keyword, "true"));
            AssertTokens("false", token => token.ShouldBe(TokenKind.Keyword, "false"));
            AssertTokens("int", token => token.ShouldBe(TokenKind.Keyword, "int"));
            AssertTokens("bool", token => token.ShouldBe(TokenKind.Keyword, "bool"));
            AssertTokens("void", token => token.ShouldBe(TokenKind.Keyword, "void"));
            AssertTokens("null", token => token.ShouldBe(TokenKind.Keyword, "null"));
            AssertTokens("if", token => token.ShouldBe(TokenKind.Keyword, "if"));
            AssertTokens("return", token => token.ShouldBe(TokenKind.Keyword, "return"));
            AssertTokens("else", token => token.ShouldBe(TokenKind.Keyword, "else"));
            AssertTokens("fn", token => token.ShouldBe(TokenKind.Keyword, "fn"));
        }

        [Test]
        public void ShouldRecognizeIdentifiers()
        {
            AssertTokens("a", token => token.ShouldBe(TokenKind.Identifier, "a"));
            AssertTokens("ab", token => token.ShouldBe(TokenKind.Identifier, "ab"));
            AssertTokens("a0", token => token.ShouldBe(TokenKind.Identifier, "a0"));
        }

        [Test]
        public void ShouldRecognizeOperators()
        {
            AssertTokens("<=>=<>!====*/+-&&||!{}[][,]()???:",
                token => token.ShouldBe(TokenKind.Operator, "<="),
                token => token.ShouldBe(TokenKind.Operator, ">="),
                token => token.ShouldBe(TokenKind.Operator, "<"),
                token => token.ShouldBe(TokenKind.Operator, ">"),
                token => token.ShouldBe(TokenKind.Operator, "!="),
                token => token.ShouldBe(TokenKind.Operator, "=="),
                token => token.ShouldBe(TokenKind.Operator, "="),
                token => token.ShouldBe(TokenKind.Operator, "*"),
                token => token.ShouldBe(TokenKind.Operator, "/"),
                token => token.ShouldBe(TokenKind.Operator, "+"),
                token => token.ShouldBe(TokenKind.Operator, "-"),
                token => token.ShouldBe(TokenKind.Operator, "&&"),
                token => token.ShouldBe(TokenKind.Operator, "||"),
                token => token.ShouldBe(TokenKind.Operator, "!"),
                token => token.ShouldBe(TokenKind.Operator, "{"),
                token => token.ShouldBe(TokenKind.Operator, "}"),
                token => token.ShouldBe(TokenKind.Operator, "[]"),
                token => token.ShouldBe(TokenKind.Operator, "["),
                token => token.ShouldBe(TokenKind.Operator, ","),
                token => token.ShouldBe(TokenKind.Operator, "]"),
                token => token.ShouldBe(TokenKind.Operator, "("),
                token => token.ShouldBe(TokenKind.Operator, ")"),
                token => token.ShouldBe(TokenKind.Operator, "??"),
                token => token.ShouldBe(TokenKind.Operator, "?"),
                token => token.ShouldBe(TokenKind.Operator, ":"));
        }

        [Test]
        public void ShouldRecognizeEndOfLogicalLine()
        {
            //Endlines are \r\n or semicolons (with optional preceding spaces/tabs and optional trailing whitspace).

            AssertTokens("\r\n", token => token.ShouldBe(TokenKind.EndOfLine, "\r\n"));
            AssertTokens("\r\n \r\n \t ", token => token.ShouldBe(TokenKind.EndOfLine, "\r\n \r\n \t "));

            AssertTokens(";", token => token.ShouldBe(TokenKind.EndOfLine, ";"));
            AssertTokens("; \r\n \t ", token => token.ShouldBe(TokenKind.EndOfLine, "; \r\n \t "));
        }

        private static void AssertTokens(string source, params Action<Token>[] assertions)
        {
            Lexer lexer = new RookLexer(source);

            foreach (var assertToken in assertions)
            {
                assertToken(lexer.CurrentToken);
                lexer = lexer.Advance();
            }

            lexer.CurrentToken.Kind.ShouldEqual(Parsley.TokenKind.EndOfInput);
        }
    }
}