using System.Linq;
using Parsley;
using NUnit.Framework;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class TokenParserSpec
    {
        private static readonly string[] expectedKeywords = new[]
        {
            "true", "false", "int", "bool", "void", "null", 
            "if", "return", "else", "fn"
        };

        [Test]
        public void ParsesIntegerValues()
        {
            AssertParse(Grammar.Integer, TokenKind.Integer, "0", "0");
            AssertParse(Grammar.Integer, TokenKind.Integer, "0", " \t 0");

            //NOTE: Rook integer literals are not (yet) limited to int.MaxValue:
            AssertParse(Grammar.Integer, TokenKind.Integer, "2147483648", "2147483648");
        }

        [Test]
        public void ParsesBooleanValues()
        {
            AssertParse(Grammar.Boolean, TokenKind.Keyword, "false", "false");
            AssertParse(Grammar.Boolean, TokenKind.Keyword, "true", "true");
            AssertParse(Grammar.Boolean, TokenKind.Keyword, "false", " \t false");
            AssertParse(Grammar.Boolean, TokenKind.Keyword, "true", " \t true");
        }

        [Test]
        public void ParsesOperatorsGreedily()
        {
            AssertTokens(Grammar.AnyOperator,
                         " \t <=>=<>!====*/+-&&||!{}[][,]()???:",
                         "<=", ">=", "<", ">", "!=", "==", "=", "*", "/", "+", "-",
                         "&&", "||", "!", "{", "}", "[]", "[", ",", "]", "(", ")", "??", "?", ":");
            Grammar.AnyOperator.FailsToParse(Tokenize("0"), "0");
        }

        [Test]
        public void ParsesExpectedOperators()
        {
            AssertParse(Grammar.Operator("<", "="), TokenKind.Operator, "=", "=");
            AssertParse(Grammar.Operator("<", "="), TokenKind.Operator, "<", "<");
            AssertParse(Grammar.Operator("<", "="), TokenKind.Operator, "=", " \t =");

            AssertParse(Grammar.Operator("<=", "<", "="), TokenKind.Operator, "<=", "<=");
            Grammar.Operator("<=", "<", "=").FailsToParse(Tokenize("!"), "!").WithMessage("(1, 1): <=, <, = expected");
            Grammar.Operator("<", "=").FailsToParse(Tokenize("<="), "<=").WithMessage("(1, 1): <, = expected");
        }

        [Test]
        public void ParsesKeywords()
        {
            AssertTokens(Grammar.AnyKeyword,
                         " \t true false int bool void null if return else fn",
                         expectedKeywords);
            Grammar.AnyKeyword.FailsToParse(Tokenize("iftrue"), "iftrue");
            Grammar.AnyKeyword.FailsToParse(Tokenize("random text"), "random text");
        }

        [Test]
        public void ParsesExpectedKeyword()
        {
            var ifOrTrue = Grammar.Keyword("if", "true");
            AssertParse(ifOrTrue, TokenKind.Keyword, "true", "true");
            AssertParse(ifOrTrue, TokenKind.Keyword, "if", "if");
            AssertParse(ifOrTrue, TokenKind.Keyword, "true", " \t true");
            ifOrTrue.FailsToParse(Tokenize("iftrue"), "iftrue");
            ifOrTrue.FailsToParse(Tokenize("random text"), "random text");
        }

        [Test]
        public void ParsesIdentifiers()
        {
            AssertParse(Grammar.Identifier, TokenKind.Identifier, "a", "a");
            AssertParse(Grammar.Identifier, TokenKind.Identifier, "a", " \t a");
            AssertParse(Grammar.Identifier, TokenKind.Identifier, "ab", "ab");
            AssertParse(Grammar.Identifier, TokenKind.Identifier, "a0", "a0");
            AssertParse(Grammar.Identifier, TokenKind.Identifier, "a01", "a01");

            Grammar.Identifier.FailsToParse(Tokenize("0"), "0");
            foreach (string keyword in expectedKeywords)
                Grammar.Identifier.FailsToParse(Tokenize(keyword), keyword);
        }

        [Test]
        public void ParsesLineEndings()
        {
            //Endlines are the end of input, \r\n, or semicolons (with optional preceding spaces/tabs and optional trailing whitspace).

            AssertParse(Grammar.EndOfLine, Parsley.TokenKind.EndOfInput, "", "");
            AssertParse(Grammar.EndOfLine, Parsley.TokenKind.EndOfInput, "", " \t \t");

            AssertParse(Grammar.EndOfLine, TokenKind.EndOfLine, "\r\n", "\r\n");
            AssertParse(Grammar.EndOfLine, TokenKind.EndOfLine, "\r\n", " \t \t\r\n");
            AssertParse(Grammar.EndOfLine, TokenKind.EndOfLine, "\r\n \r\n \t ", "\r\n \r\n \t ");
            AssertParse(Grammar.EndOfLine, TokenKind.EndOfLine, "\r\n \r\n \t ", " \t \t\r\n \r\n \t ");

            AssertParse(Grammar.EndOfLine, TokenKind.EndOfLine, ";", ";");
            AssertParse(Grammar.EndOfLine, TokenKind.EndOfLine, ";", " \t \t;");
            AssertParse(Grammar.EndOfLine, TokenKind.EndOfLine, "; \r\n \t ", "; \r\n \t ");
            AssertParse(Grammar.EndOfLine, TokenKind.EndOfLine, "; \r\n \t ", " \t \t; \r\n \t ");

            AssertError(Grammar.EndOfLine, "x", "x", "(1, 1): end of line expected");
            AssertError(Grammar.EndOfLine, " x", "x", "(1, 2): end of line expected");
        }

        private static Lexer Tokenize(string source)
        {
            return new RookLexer(source);
        }

        private static void AssertTokens(Parser<Token> parse, string source, params string[] expectedTokens)
        {
            AbstractGrammar.ZeroOrMore(parse).Parses(Tokenize(source))
                .IntoValue(parsedValues => parsedValues.Select(x => x.Literal).ShouldList(expectedTokens));
        }

        private static void AssertParse(Parser<Token> parse, Parsley.TokenKind expectedKind, string expectedValue, string source)
        {
            parse.Parses(Tokenize(source)).IntoValue(parsedValue =>
            {
                parsedValue.Kind.ShouldEqual(expectedKind);
                parsedValue.Literal.ShouldEqual(expectedValue);
            });
        }

        private static void AssertError<T>(Parser<T> parse, string source, string expectedUnparsedSource, string expectedMessage)
        {
            parse.FailsToParse(Tokenize(source), expectedUnparsedSource).WithMessage(expectedMessage);
        }
    }
}