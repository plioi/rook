using Parsley;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class TokenParserTests : RookGrammar
    {
        [Fact]
        public void ParsesExpectedOperators()
        {
            var operators = new[]
            {
                "(", ")", "*", "/", "+", "-", "<=", "<", ">=", ">", "!=", "==", "=",
                "&&", "||", "!", ",", "{", "}", "[]", ":", "[", "]", "??", "?"
            };

            foreach (var o in operators)
            {
                Token(o).Parses(o).IntoToken(o).Value.Kind.ShouldBeType<Operator>();
                Token(o).Parses(o + " \t ").IntoToken(o).Value.Kind.ShouldBeType<Operator>();
                Token(o).FailsToParse("x").LeavingUnparsedTokens("x").WithMessage("(1, 1): " + o + " expected");
            }
        }

        [Fact]
        public void ParsesIdentifiers()
        {
            Identifier.Parses("a").IntoToken(RookLexer.Identifier, "a");
            Identifier.Parses("a \t ").IntoToken(RookLexer.Identifier, "a");
            Identifier.Parses("ab").IntoToken(RookLexer.Identifier, "ab");
            Identifier.Parses("a0").IntoToken(RookLexer.Identifier, "a0");
            Identifier.Parses("a01").IntoToken(RookLexer.Identifier, "a01");

            var keywords = new[] {"true", "false", "int", "bool", "void", "null", "if", "else", "fn"};

            Identifier.FailsToParse("0").LeavingUnparsedTokens("0");
            foreach (string keyword in keywords)
                Identifier.FailsToParse(keyword).LeavingUnparsedTokens(keyword);
        }

        [Fact]
        public void ParsesLineEndings()
        {
            //Endlines are the end of input, \n, or semicolons (with optional trailing whitespace).
            //Note that Parsley normalizes \r, \n, and \r\n to a single line feed \n.

            EndOfLine.Parses("").IntoToken(TokenKind.EndOfInput, "");

            EndOfLine.Parses("\r\n").IntoToken(RookLexer.EndOfLine, "\n");
            EndOfLine.Parses("\r\n \t \t").IntoToken(RookLexer.EndOfLine, "\n \t \t");
            EndOfLine.Parses("\r\n \r\n \t ").IntoToken(RookLexer.EndOfLine, "\n \n \t ");

            EndOfLine.Parses(";").IntoToken(RookLexer.EndOfLine, ";");
            EndOfLine.Parses("; \t \t").IntoToken(RookLexer.EndOfLine, "; \t \t");
            EndOfLine.Parses("; \r\n \t ").IntoToken(RookLexer.EndOfLine, "; \n \t ");

            EndOfLine.FailsToParse("x").LeavingUnparsedTokens("x").WithMessage("(1, 1): end of line expected");
        }
    }
}