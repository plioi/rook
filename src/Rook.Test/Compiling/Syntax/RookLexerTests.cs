using NUnit.Framework;
using Parsley;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class RookLexerTests : Grammar
    {
        [Test]
        public void ShouldRecognizeIntegers()
        {
            new RookLexer("0").ShouldYieldTokens(RookLexer.Integer, "0");
            
            //NOTE: Integer literals are not (yet) limited to int.MaxValue:
            new RookLexer("2147483648").ShouldYieldTokens(RookLexer.Integer, "2147483648");
        }

        [Test]
        public void ShouldRecognizeKeywords()
        {
            new RookLexer("true").ShouldYieldTokens(RookLexer.@true, "true");
            new RookLexer("false").ShouldYieldTokens(RookLexer.@false, "false");
            new RookLexer("int").ShouldYieldTokens(RookLexer.@int, "int");
            new RookLexer("bool").ShouldYieldTokens(RookLexer.@bool, "bool");
            new RookLexer("void").ShouldYieldTokens(RookLexer.@void, "void");
            new RookLexer("null").ShouldYieldTokens(RookLexer.@null, "null");
            new RookLexer("if").ShouldYieldTokens(RookLexer.@if, "if");
            new RookLexer("else").ShouldYieldTokens(RookLexer.@else, "else");
            new RookLexer("fn").ShouldYieldTokens(RookLexer.@fn, "fn");
        }

        [Test]
        public void ShouldRecognizeIdentifiers()
        {
            new RookLexer("a").ShouldYieldTokens(RookLexer.Identifier, "a");
            new RookLexer("ab").ShouldYieldTokens(RookLexer.Identifier, "ab");
            new RookLexer("a0").ShouldYieldTokens(RookLexer.Identifier, "a0");
        }

        [Test]
        public void ShouldRecognizeOperatorsGreedily()
        {
            new RookLexer("<=>=<>!====*/+-&&||!{}[][,]()???:").ShouldYieldTokens("<=", ">=", "<", ">", "!=", "==", "=", "*", "/", "+", "-", "&&", "||", "!", "{", "}", "[]", "[", ",", "]", "(", ")", "??", "?", ":");
        }

        [Test]
        public void ShouldRecognizeEndOfLogicalLine()
        {
            //Endlines are \r\n or semicolons (with optional preceding spaces/tabs and optional trailing whitspace).

            new RookLexer("\r\n").ShouldYieldTokens(RookLexer.EndOfLine, "\r\n");
            new RookLexer("\r\n \r\n \t ").ShouldYieldTokens(RookLexer.EndOfLine, "\r\n \r\n \t ");

            new RookLexer(";").ShouldYieldTokens(RookLexer.EndOfLine, ";");
            new RookLexer("; \r\n \t ").ShouldYieldTokens(RookLexer.EndOfLine, "; \r\n \t ");
        }

        [Test]
        public void ShouldRecognizeAndSkipOverIntralineWhitespace()
        {
            new RookLexer(" a if == \r\n 0 ").ShouldYieldTokens("a", "if", "==", "\r\n ", "0");
            new RookLexer("\ta\tif\t==\t\r\n\t0\t").ShouldYieldTokens("a", "if", "==", "\r\n\t", "0");
            new RookLexer(" \t a \t if \t == \t \r\n \t 0 \t ").ShouldYieldTokens("a", "if", "==", "\r\n \t ", "0");
            new RookLexer("\t \ta\t \tif\t \t==\t \t\r\n\t \t0\t \t").ShouldYieldTokens("a", "if", "==", "\r\n\t \t", "0");
        }
    }
}