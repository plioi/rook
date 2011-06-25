using NUnit.Framework;
using Parsley;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class RookLexerSpec : AbstractGrammar
    {
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
            AssertTokens("true", RookLexer.@true, "true");
            AssertTokens("false", RookLexer.@false, "false");
            AssertTokens("int", RookLexer.@int, "int");
            AssertTokens("bool", RookLexer.@bool, "bool");
            AssertTokens("void", RookLexer.@void, "void");
            AssertTokens("null", RookLexer.@null, "null");
            AssertTokens("if", RookLexer.@if, "if");
            AssertTokens("else", RookLexer.@else, "else");
            AssertTokens("fn", RookLexer.@fn, "fn");
        }

        [Test]
        public void ShouldRecognizeIdentifiers()
        {
            AssertTokens("a", RookLexer.Identifier, "a");
            AssertTokens("ab", RookLexer.Identifier, "ab");
            AssertTokens("a0", RookLexer.Identifier, "a0");
        }

        [Test]
        public void ShouldRecognizeOperatorsGreedily()
        {
            AssertTokens("<=>=<>!====*/+-&&||!{}[][,]()???:", "<=",
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

        [Test]
        public void ShouldRecognizeAndSkipOverIntralineWhiteSpaces()
        {
            AssertTokens(" a if == \r\n 0 ", "a", "if", "==", "\r\n ", "0");
            AssertTokens("\ta\tif\t==\t\r\n\t0\t", "a", "if", "==", "\r\n\t", "0");
            AssertTokens(" \t a \t if \t == \t \r\n \t 0 \t ", "a", "if", "==", "\r\n \t ", "0");
            AssertTokens("\t \ta\t \tif\t \t==\t \t\r\n\t \t0\t \t", "a", "if", "==", "\r\n\t \t", "0");
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

        private static void AssertTokens(string source, params string[] expectedLiterals)
        {
            Lexer lexer = new RookLexer(source);

            foreach (var expectedLiteral in expectedLiterals)
            {
                lexer.CurrentToken.Literal.ShouldEqual(expectedLiteral);
                lexer = lexer.Advance();
            }

            lexer.CurrentToken.Kind.ShouldEqual(Lexer.EndOfInput);
        }
    }
}