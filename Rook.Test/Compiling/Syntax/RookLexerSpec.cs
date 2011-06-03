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
            AssertTokens(" ", token => token.AssertToken(TokenKind.IntralineWhiteSpace, " ", 1, 1));
            AssertTokens("\t", token => token.AssertToken(TokenKind.IntralineWhiteSpace, "\t", 1, 1));
            AssertTokens(" \t ", token => token.AssertToken(TokenKind.IntralineWhiteSpace, " \t ", 1, 1));
            AssertTokens("\t \t", token => token.AssertToken(TokenKind.IntralineWhiteSpace, "\t \t", 1, 1));
        }

        [Test]
        public void ShouldRecognizeIntegers()
        {
            AssertTokens("0", token => token.AssertToken(TokenKind.Integer, "0", 1, 1));
            
            //NOTE: Integer literals are not (yet) limited to int.MaxValue:
            AssertTokens("2147483648", token => token.AssertToken(TokenKind.Integer, "2147483648", 1, 1));
        }

        [Test]
        public void ShouldRecognizeKeywords()
        {
            AssertTokens("true", token => token.AssertToken(TokenKind.Keyword, "true", 1, 1));
            AssertTokens("false", token => token.AssertToken(TokenKind.Keyword, "false", 1, 1));
            AssertTokens("int", token => token.AssertToken(TokenKind.Keyword, "int", 1, 1));
            AssertTokens("bool", token => token.AssertToken(TokenKind.Keyword, "bool", 1, 1));
            AssertTokens("void", token => token.AssertToken(TokenKind.Keyword, "void", 1, 1));
            AssertTokens("null", token => token.AssertToken(TokenKind.Keyword, "null", 1, 1));
            AssertTokens("if", token => token.AssertToken(TokenKind.Keyword, "if", 1, 1));
            AssertTokens("return", token => token.AssertToken(TokenKind.Keyword, "return", 1, 1));
            AssertTokens("else", token => token.AssertToken(TokenKind.Keyword, "else", 1, 1));
            AssertTokens("fn", token => token.AssertToken(TokenKind.Keyword, "fn", 1, 1));
        }

        [Test]
        public void ShouldRecognizeIdentifiers()
        {
            AssertTokens("a", token => token.AssertToken(TokenKind.Identifier, "a", 1, 1));
            AssertTokens("ab", token => token.AssertToken(TokenKind.Identifier, "ab", 1, 1));
            AssertTokens("a0", token => token.AssertToken(TokenKind.Identifier, "a0", 1, 1));
        }

        [Test]
        public void ShouldRecognizeOperators()
        {
            AssertTokens("<=>=<>!====*/+-&&||!{}[][,]()???:",
                token => token.AssertToken(TokenKind.Operator, "<=", 1, 1),
                token => token.AssertToken(TokenKind.Operator, ">=", 1, 3),
                token => token.AssertToken(TokenKind.Operator, "<", 1, 5),
                token => token.AssertToken(TokenKind.Operator, ">", 1, 6),
                token => token.AssertToken(TokenKind.Operator, "!=", 1, 7),
                token => token.AssertToken(TokenKind.Operator, "==", 1, 9),
                token => token.AssertToken(TokenKind.Operator, "=", 1, 11),
                token => token.AssertToken(TokenKind.Operator, "*", 1, 12),
                token => token.AssertToken(TokenKind.Operator, "/", 1, 13),
                token => token.AssertToken(TokenKind.Operator, "+", 1, 14),
                token => token.AssertToken(TokenKind.Operator, "-", 1, 15),
                token => token.AssertToken(TokenKind.Operator, "&&", 1, 16),
                token => token.AssertToken(TokenKind.Operator, "||", 1, 18),
                token => token.AssertToken(TokenKind.Operator, "!", 1, 20),
                token => token.AssertToken(TokenKind.Operator, "{", 1, 21),
                token => token.AssertToken(TokenKind.Operator, "}", 1, 22),
                token => token.AssertToken(TokenKind.Operator, "[]", 1, 23),
                token => token.AssertToken(TokenKind.Operator, "[", 1, 25),
                token => token.AssertToken(TokenKind.Operator, ",", 1, 26),
                token => token.AssertToken(TokenKind.Operator, "]", 1, 27),
                token => token.AssertToken(TokenKind.Operator, "(", 1, 28),
                token => token.AssertToken(TokenKind.Operator, ")", 1, 29),
                token => token.AssertToken(TokenKind.Operator, "??", 1, 30),
                token => token.AssertToken(TokenKind.Operator, "?", 1, 32),
                token => token.AssertToken(TokenKind.Operator, ":", 1, 33));
        }

        [Test]
        public void ShouldRecognizeEndOfLogicalLine()
        {
            //Endlines are \r\n or semicolons (with optional preceding spaces/tabs and optional trailing whitspace).

            AssertTokens("\r\n", token => token.AssertToken(TokenKind.EndOfLine, "\r\n", 1, 1));
            AssertTokens("\r\n \r\n \t ", token => token.AssertToken(TokenKind.EndOfLine, "\r\n \r\n \t ", 1, 1));

            AssertTokens(";", token => token.AssertToken(TokenKind.EndOfLine, ";", 1, 1));
            AssertTokens("; \r\n \t ", token => token.AssertToken(TokenKind.EndOfLine, "; \r\n \t ", 1, 1));
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