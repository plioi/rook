using Parsley;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class RookLexerTests : Grammar
    {
        [Fact]
        public void ShouldRecognizeIntegers()
        {
            new RookLexer("0").ShouldYieldTokens(RookLexer.Integer, "0");
            new RookLexer("1").ShouldYieldTokens(RookLexer.Integer, "1");
            new RookLexer("01").ShouldYieldTokens(Lexer.Unknown, "01");
            new RookLexer("10").ShouldYieldTokens(RookLexer.Integer, "10");
            new RookLexer("2147483647").ShouldYieldTokens(RookLexer.Integer, "2147483647");

            //NOTE: The parser does not limit integer literals to min and max values,
            //      because that is a type checking concern.
            new RookLexer("2147483648").ShouldYieldTokens(RookLexer.Integer, "2147483648");
        }

        [Fact]
        public void ShouldRecognizeStringLiterals()
        {
            new RookLexer("\"\"").ShouldYieldTokens(RookLexer.StringLiteral, "\"\"");
            new RookLexer("\"a\"").ShouldYieldTokens(RookLexer.StringLiteral, "\"a\"");
            new RookLexer("\"abc\"").ShouldYieldTokens(RookLexer.StringLiteral, "\"abc\"");
            new RookLexer("\"abc \\\" def\"").ShouldYieldTokens(RookLexer.StringLiteral, "\"abc \\\" def\"");
            new RookLexer("\"abc \\\\ def\"").ShouldYieldTokens(RookLexer.StringLiteral, "\"abc \\\\ def\"");
            new RookLexer("\"abc \\n def\"").ShouldYieldTokens(RookLexer.StringLiteral, "\"abc \\n def\"");
            new RookLexer("\"abc \\r def\"").ShouldYieldTokens(RookLexer.StringLiteral, "\"abc \\r def\"");
            new RookLexer("\"abc \\t def\"").ShouldYieldTokens(RookLexer.StringLiteral, "\"abc \\t def\"");
            new RookLexer("\"abc \\u005C def\"").ShouldYieldTokens(RookLexer.StringLiteral, "\"abc \\u005C def\"");

            new RookLexer("\" a \" \" b \" \" c \"").ShouldYieldTokens(RookLexer.StringLiteral, "\" a \"", "\" b \"", "\" c \"");
        }

        [Fact]
        public void ShouldRecognizeKeywords()
        {
            new RookLexer("true").ShouldYieldTokens(RookLexer.@true, "true");
            new RookLexer("false").ShouldYieldTokens(RookLexer.@false, "false");
            new RookLexer("int").ShouldYieldTokens(RookLexer.@int, "int");
            new RookLexer("bool").ShouldYieldTokens(RookLexer.@bool, "bool");
            new RookLexer("string").ShouldYieldTokens(RookLexer.@string, "string");
            new RookLexer("void").ShouldYieldTokens(RookLexer.@void, "void");
            new RookLexer("null").ShouldYieldTokens(RookLexer.@null, "null");
            new RookLexer("if").ShouldYieldTokens(RookLexer.@if, "if");
            new RookLexer("else").ShouldYieldTokens(RookLexer.@else, "else");
            new RookLexer("fn").ShouldYieldTokens(RookLexer.@fn, "fn");
        }

        [Fact]
        public void ShouldRecognizeIdentifiers()
        {
            new RookLexer("a").ShouldYieldTokens(RookLexer.Identifier, "a");
            new RookLexer("ab").ShouldYieldTokens(RookLexer.Identifier, "ab");
            new RookLexer("a0").ShouldYieldTokens(RookLexer.Identifier, "a0");
        }

        [Fact]
        public void ShouldRecognizeOperatorsGreedily()
        {
            new RookLexer("<=>=<>!====*/+-&&||!{}[][,]()???:").ShouldYieldTokens("<=", ">=", "<", ">", "!=", "==", "=", "*", "/", "+", "-", "&&", "||", "!", "{", "}", "[]", "[", ",", "]", "(", ")", "??", "?", ":");

            new RookLexer("(").ShouldYieldTokens(RookLexer.LeftParen, "(");
            new RookLexer(")").ShouldYieldTokens(RookLexer.RightParen, ")");
            new RookLexer("*").ShouldYieldTokens(RookLexer.Multiply, "*");
            new RookLexer("/").ShouldYieldTokens(RookLexer.Divide, "/");
            new RookLexer("+").ShouldYieldTokens(RookLexer.Add, "+");
            new RookLexer("-").ShouldYieldTokens(RookLexer.Subtract, "-");
            new RookLexer("<=").ShouldYieldTokens(RookLexer.LessThanOrEqual, "<=");
            new RookLexer("<").ShouldYieldTokens(RookLexer.LessThan, "<");
            new RookLexer(">=").ShouldYieldTokens(RookLexer.GreaterThanOrEqual, ">=");
            new RookLexer(">").ShouldYieldTokens(RookLexer.GreaterThan, ">");
            new RookLexer("==").ShouldYieldTokens(RookLexer.Equal, "==");
            new RookLexer("!=").ShouldYieldTokens(RookLexer.NotEqual, "!=");
            new RookLexer("||").ShouldYieldTokens(RookLexer.Or, "||");
            new RookLexer("&&").ShouldYieldTokens(RookLexer.And, "&&");
            new RookLexer("!").ShouldYieldTokens(RookLexer.Not, "!");
            new RookLexer("=").ShouldYieldTokens(RookLexer.Assignment, "=");
            new RookLexer(",").ShouldYieldTokens(RookLexer.Comma, ",");
            new RookLexer("{").ShouldYieldTokens(RookLexer.LeftBrace, "{");
            new RookLexer("}").ShouldYieldTokens(RookLexer.RightBrace, "}");
            new RookLexer("[]").ShouldYieldTokens(RookLexer.Vector, "[]");
            new RookLexer("[").ShouldYieldTokens(RookLexer.LeftSquareBrace, "[");
            new RookLexer("]").ShouldYieldTokens(RookLexer.RightSquareBrace, "]");
            new RookLexer(":").ShouldYieldTokens(RookLexer.Colon, ":");
            new RookLexer("??").ShouldYieldTokens(RookLexer.NullCoalesce, "??");
            new RookLexer("?").ShouldYieldTokens(RookLexer.Question, "?");
        }

        [Fact]
        public void ShouldRecognizeEndOfLogicalLine()
        {
            //Endlines are \n or semicolons (with optional preceding spaces/tabs and optional trailing whitspace).
            //Note that Parsley normalizes \r, \n, and \r\n to a single line feed \n.

            new RookLexer("\r\n").ShouldYieldTokens(RookLexer.EndOfLine, "\n");
            new RookLexer("\r\n \r\n \t ").ShouldYieldTokens(RookLexer.EndOfLine, "\n \n \t ");

            new RookLexer(";").ShouldYieldTokens(RookLexer.EndOfLine, ";");
            new RookLexer("; \r\n \t ").ShouldYieldTokens(RookLexer.EndOfLine, "; \n \t ");
        }

        [Fact]
        public void ShouldRecognizeAndSkipOverIntralineWhitespace()
        {
            //Note that Parsley normalizes \r, \n, and \r\n to a single line feed \n.

            new RookLexer(" a if == \r\n 0 ").ShouldYieldTokens("a", "if", "==", "\n ", "0");
            new RookLexer("\ta\tif\t==\t\r\n\t0\t").ShouldYieldTokens("a", "if", "==", "\n\t", "0");
            new RookLexer(" \t a \t if \t == \t \r\n \t 0 \t ").ShouldYieldTokens("a", "if", "==", "\n \t ", "0");
            new RookLexer("\t \ta\t \tif\t \t==\t \t\r\n\t \t0\t \t").ShouldYieldTokens("a", "if", "==", "\n\t \t", "0");
        }
    }
}