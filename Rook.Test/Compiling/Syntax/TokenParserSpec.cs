using Parsley;
using NUnit.Framework;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class TokenParserSpec
    {
        private static readonly string[] expectedKeywords = new[]
        {
            "true", "false", "int", "bool", "void", "null", 
            "if", "return", "else", "fn"
        };

        [Test]
        public void ParsesIntegerValues()
        {
            Grammar.Integer.Parses("0").IntoToken(RookLexer.Integer, "0");
            Grammar.Integer.Parses(" \t 0").IntoToken(RookLexer.Integer, "0");

            //NOTE: Rook integer literals are not (yet) limited to int.MaxValue:
            Grammar.Integer.Parses("2147483648").IntoToken(RookLexer.Integer, "2147483648");
        }

        [Test]
        public void ParsesBooleanValues()
        {
            Grammar.Boolean.Parses("false").IntoToken(RookLexer.Keyword, "false");
            Grammar.Boolean.Parses("true").IntoToken(RookLexer.Keyword, "true");
            Grammar.Boolean.Parses(" \t false").IntoToken(RookLexer.Keyword, "false");
            Grammar.Boolean.Parses(" \t true").IntoToken(RookLexer.Keyword, "true");
        }

        [Test]
        public void ParsesOperatorsGreedily()
        {
            AbstractGrammar.ZeroOrMore(Grammar.AnyOperator)
                .Parses(" \t <=>=<>!====*/+-&&||!{}[][,]()???:")
                .IntoTokens("<=", ">=", "<", ">", "!=", "==", "=", "*", "/", "+", "-",
                            "&&", "||", "!", "{", "}", "[]", "[", ",", "]", "(", ")", "??", "?", ":");
            Grammar.AnyOperator.FailsToParse("0", "0");
        }

        [Test]
        public void ParsesExpectedOperators()
        {
            Grammar.Operator("<", "=").Parses("=").IntoToken(RookLexer.Operator, "=");
            Grammar.Operator("<", "=").Parses("<").IntoToken(RookLexer.Operator, "<");
            Grammar.Operator("<", "=").Parses(" \t =").IntoToken(RookLexer.Operator, "=");

            Grammar.Operator("<=", "<", "=").Parses("<=").IntoToken(RookLexer.Operator, "<=");
            Grammar.Operator("<=", "<", "=").FailsToParse("!", "!").WithMessage("(1, 1): <=, <, = expected");
            Grammar.Operator("<", "=").FailsToParse("<=", "<=").WithMessage("(1, 1): <, = expected");
        }

        [Test]
        public void ParsesKeywords()
        {
            AbstractGrammar.ZeroOrMore(Grammar.AnyKeyword)
                .Parses(" \t true false int bool void null if return else fn")
                .IntoTokens(expectedKeywords);
            Grammar.AnyKeyword.FailsToParse("iftrue", "iftrue");
            Grammar.AnyKeyword.FailsToParse("random text", "random text");
        }

        [Test]
        public void ParsesExpectedKeyword()
        {
            var ifOrTrue = Grammar.Keyword("if", "true");
            ifOrTrue.Parses("true").IntoToken(RookLexer.Keyword, "true");
            ifOrTrue.Parses("if").IntoToken(RookLexer.Keyword, "if");
            ifOrTrue.Parses(" \t true").IntoToken(RookLexer.Keyword, "true");
            ifOrTrue.FailsToParse("iftrue", "iftrue");
            ifOrTrue.FailsToParse("random text", "random text");
        }

        [Test]
        public void ParsesIdentifiers()
        {
            Grammar.Identifier.Parses("a").IntoToken(RookLexer.Identifier, "a");
            Grammar.Identifier.Parses(" \t a").IntoToken(RookLexer.Identifier, "a");
            Grammar.Identifier.Parses("ab").IntoToken(RookLexer.Identifier, "ab");
            Grammar.Identifier.Parses("a0").IntoToken(RookLexer.Identifier, "a0");
            Grammar.Identifier.Parses("a01").IntoToken(RookLexer.Identifier, "a01");

            Grammar.Identifier.FailsToParse("0", "0");
            foreach (string keyword in expectedKeywords)
                Grammar.Identifier.FailsToParse(keyword, keyword);
        }

        [Test]
        public void ParsesLineEndings()
        {
            //Endlines are the end of input, \r\n, or semicolons (with optional preceding spaces/tabs and optional trailing whitspace).

            Grammar.EndOfLine.Parses("").IntoToken(Lexer.EndOfInput, "");
            Grammar.EndOfLine.Parses(" \t \t").IntoToken(Lexer.EndOfInput, "");

            Grammar.EndOfLine.Parses("\r\n").IntoToken(RookLexer.EndOfLine, "\r\n");
            Grammar.EndOfLine.Parses(" \t \t\r\n").IntoToken(RookLexer.EndOfLine, "\r\n");
            Grammar.EndOfLine.Parses("\r\n \r\n \t ").IntoToken(RookLexer.EndOfLine, "\r\n \r\n \t ");
            Grammar.EndOfLine.Parses(" \t \t\r\n \r\n \t ").IntoToken(RookLexer.EndOfLine, "\r\n \r\n \t ");

            Grammar.EndOfLine.Parses(";").IntoToken(RookLexer.EndOfLine, ";");
            Grammar.EndOfLine.Parses(" \t \t;").IntoToken(RookLexer.EndOfLine, ";");
            Grammar.EndOfLine.Parses("; \r\n \t ").IntoToken(RookLexer.EndOfLine, "; \r\n \t ");
            Grammar.EndOfLine.Parses(" \t \t; \r\n \t ").IntoToken(RookLexer.EndOfLine, "; \r\n \t ");

            Grammar.EndOfLine.FailsToParse("x", "x").WithMessage("(1, 1): end of line expected");
            Grammar.EndOfLine.FailsToParse(" x", "x").WithMessage("(1, 2): end of line expected");
        }
    }
}