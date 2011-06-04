using System;
using NUnit.Framework;
using Parsley;
using Text = Parsley.Text;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class RookLexerSpec : AbstractGrammar
    {
        [Test]
        public void ShouldRecognizeIntralineWhiteSpaces()
        {
            AssertTokens(" ", token => token.AssertToken(TokenKind.IntralineWhiteSpace, " "));
            AssertTokens("\t", token => token.AssertToken(TokenKind.IntralineWhiteSpace, "\t"));
            AssertTokens(" \t ", token => token.AssertToken(TokenKind.IntralineWhiteSpace, " \t "));
            AssertTokens("\t \t", token => token.AssertToken(TokenKind.IntralineWhiteSpace, "\t \t"));
        }

        [Test]
        public void ShouldRecognizeIntegers()
        {
            AssertTokens("0", token => token.AssertToken(TokenKind.Integer, "0"));
            
            //NOTE: Integer literals are not (yet) limited to int.MaxValue:
            AssertTokens("2147483648", token => token.AssertToken(TokenKind.Integer, "2147483648"));
        }

        [Test]
        public void ShouldRecognizeKeywords()
        {
            AssertTokens("true", token => token.AssertToken(TokenKind.Keyword, "true"));
            AssertTokens("false", token => token.AssertToken(TokenKind.Keyword, "false"));
            AssertTokens("int", token => token.AssertToken(TokenKind.Keyword, "int"));
            AssertTokens("bool", token => token.AssertToken(TokenKind.Keyword, "bool"));
            AssertTokens("void", token => token.AssertToken(TokenKind.Keyword, "void"));
            AssertTokens("null", token => token.AssertToken(TokenKind.Keyword, "null"));
            AssertTokens("if", token => token.AssertToken(TokenKind.Keyword, "if"));
            AssertTokens("return", token => token.AssertToken(TokenKind.Keyword, "return"));
            AssertTokens("else", token => token.AssertToken(TokenKind.Keyword, "else"));
            AssertTokens("fn", token => token.AssertToken(TokenKind.Keyword, "fn"));
        }

        [Test]
        public void ShouldRecognizeIdentifiers()
        {
            AssertTokens("a", token => token.AssertToken(TokenKind.Identifier, "a"));
            AssertTokens("ab", token => token.AssertToken(TokenKind.Identifier, "ab"));
            AssertTokens("a0", token => token.AssertToken(TokenKind.Identifier, "a0"));
        }

        [Test]
        public void ShouldRecognizeOperators()
        {
            AssertTokens("<=>=<>!====*/+-&&||!{}[][,]()???:",
                token => token.AssertToken(TokenKind.Operator, "<="),
                token => token.AssertToken(TokenKind.Operator, ">="),
                token => token.AssertToken(TokenKind.Operator, "<"),
                token => token.AssertToken(TokenKind.Operator, ">"),
                token => token.AssertToken(TokenKind.Operator, "!="),
                token => token.AssertToken(TokenKind.Operator, "=="),
                token => token.AssertToken(TokenKind.Operator, "="),
                token => token.AssertToken(TokenKind.Operator, "*"),
                token => token.AssertToken(TokenKind.Operator, "/"),
                token => token.AssertToken(TokenKind.Operator, "+"),
                token => token.AssertToken(TokenKind.Operator, "-"),
                token => token.AssertToken(TokenKind.Operator, "&&"),
                token => token.AssertToken(TokenKind.Operator, "||"),
                token => token.AssertToken(TokenKind.Operator, "!"),
                token => token.AssertToken(TokenKind.Operator, "{"),
                token => token.AssertToken(TokenKind.Operator, "}"),
                token => token.AssertToken(TokenKind.Operator, "[]"),
                token => token.AssertToken(TokenKind.Operator, "["),
                token => token.AssertToken(TokenKind.Operator, ","),
                token => token.AssertToken(TokenKind.Operator, "]"),
                token => token.AssertToken(TokenKind.Operator, "("),
                token => token.AssertToken(TokenKind.Operator, ")"),
                token => token.AssertToken(TokenKind.Operator, "??"),
                token => token.AssertToken(TokenKind.Operator, "?"),
                token => token.AssertToken(TokenKind.Operator, ":"));
        }

        [Test]
        public void ShouldRecognizeEndOfLogicalLine()
        {
            //Endlines are \r\n or semicolons (with optional preceding spaces/tabs and optional trailing whitspace).

            AssertTokens("\r\n", token => token.AssertToken(TokenKind.EndOfLine, "\r\n"));
            AssertTokens("\r\n \r\n \t ", token => token.AssertToken(TokenKind.EndOfLine, "\r\n \r\n \t "));

            AssertTokens(";", token => token.AssertToken(TokenKind.EndOfLine, ";"));
            AssertTokens("; \r\n \t ", token => token.AssertToken(TokenKind.EndOfLine, "; \r\n \t "));
        }

        private static void AssertTokens(string source, params Action<Token>[] assertions)
        {
            Lexer lexer = new RookLexer(new Text(source));

            foreach (var assertToken in assertions)
            {
                assertToken(lexer.CurrentToken);
                lexer = lexer.Advance();
            }

            lexer.CurrentToken.Kind.ShouldEqual(Parsley.TokenKind.EndOfInput);
        }
    }
}