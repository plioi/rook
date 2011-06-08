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
            AssertTokens(" ", RookLexer.IntralineWhiteSpace, " ");
            AssertTokens("\t", RookLexer.IntralineWhiteSpace, "\t");
            AssertTokens(" \t ", RookLexer.IntralineWhiteSpace, " \t ");
            AssertTokens("\t \t", RookLexer.IntralineWhiteSpace, "\t \t");
        }

        [Test]
        public void ShouldRecognizeIntegers()
        {
            AssertTokens("0", RookLexer.Integer, "0");
            
            //NOTE: Integer literals are not (yet) limited to int.MaxValue:
            AssertTokens("2147483648", RookLexer.Integer, "2147483648");
        }

        [Test]
        public void ShouldRecognizeKeywords()
        {
            AssertTokens("true", RookLexer.Keyword, "true");
            AssertTokens("false", RookLexer.Keyword, "false");
            AssertTokens("int", RookLexer.Keyword, "int");
            AssertTokens("bool", RookLexer.Keyword, "bool");
            AssertTokens("void", RookLexer.Keyword, "void");
            AssertTokens("null", RookLexer.Keyword, "null");
            AssertTokens("if", RookLexer.Keyword, "if");
            AssertTokens("return", RookLexer.Keyword, "return");
            AssertTokens("else", RookLexer.Keyword, "else");
            AssertTokens("fn", RookLexer.Keyword, "fn");
        }

        [Test]
        public void ShouldRecognizeIdentifiers()
        {
            AssertTokens("a", RookLexer.Identifier, "a");
            AssertTokens("ab", RookLexer.Identifier, "ab");
            AssertTokens("a0", RookLexer.Identifier, "a0");
        }

        [Test]
        public void ShouldRecognizeOperators()
        {
            AssertTokens("<=>=<>!====*/+-&&||!{}[][,]()???:", RookLexer.Operator, "<=",
                         ">=", "<", ">", "!=", "==", "=", "*", "/",
                         "+", "-", "&&", "||", "!", "{", "}", "[]",
                         "[", ",", "]", "(", ")", "??", "?", ":");
        }

        [Test]
        public void ShouldRecognizeEndOfLogicalLine()
        {
            //Endlines are \r\n or semicolons (with optional preceding spaces/tabs and optional trailing whitspace).

            AssertTokens("\r\n", RookLexer.EndOfLine, "\r\n");
            AssertTokens("\r\n \r\n \t ", RookLexer.EndOfLine, "\r\n \r\n \t ");

            AssertTokens(";", RookLexer.EndOfLine, ";");
            AssertTokens("; \r\n \t ",RookLexer.EndOfLine, "; \r\n \t ");
        }

        private static void AssertTokens(string source, TokenKind expectedKind, params string[] expectedLiterals)
        {
            Lexer lexer = new RookLexer(source);

            foreach (var expectedLiteral in expectedLiterals)
            {
                lexer.CurrentToken.ShouldBe(expectedKind, expectedLiteral);
                lexer = lexer.Advance();
            }

            lexer.CurrentToken.Kind.ShouldEqual(Lexer.EndOfInput);
        }
    }
}