using System;
using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public static class ParsingAssertions
    {
        public static void ShouldBe(this Token actual, TokenKind expectedKind, string expectedLiteral, int expectedLine, int expectedColumn)
        {
            actual.ShouldBe(expectedKind, expectedLiteral);
            actual.Position.Line.ShouldEqual(expectedLine);
            actual.Position.Column.ShouldEqual(expectedColumn);
        }

        public static void ShouldBe(this Token actual, TokenKind expectedKind, string expectedLiteral)
        {
            actual.Kind.ShouldEqual(expectedKind);
            actual.Literal.ShouldEqual(expectedLiteral);
        }

        public static Reply<T> FailsToParse<T>(this Parser<T> parse, Lexer tokens, string expectedUnparsedSource)
        {
            return parse(tokens).Fails().WithUnparsedText(expectedUnparsedSource);
        }

        private static Reply<T> Fails<T>(this Reply<T> result)
        {
            result.Success.ShouldBeFalse("Parse completed without expected error.");

            return result;
        }

        public static Reply<T> WithMessage<T>(this Reply<T> result, string expectedMessage)
        {
            var position = result.UnparsedTokens.Position;
            var actual = String.Format("({0}, {1}): {2}", position.Line, position.Column, result.ErrorMessages);
            actual.ShouldEqual(expectedMessage);
            return result;
        }

        public static Reply<T> WithNoMessage<T>(this Reply<T> result)
        {
            result.ErrorMessages.ShouldEqual(ErrorMessageList.Empty);
            return result;
        }

        public static Reply<T> PartiallyParses<T>(this Parser<T> parse, Lexer tokens, string expectedUnparsedSource)
        {
            return parse(tokens).Succeeds().WithUnparsedText(expectedUnparsedSource);
        }

        public static Reply<T> Parses<T>(this Parser<T> parse, Lexer tokens)
        {
            return parse(tokens).Succeeds().WithAllInputConsumed();
        }

        private static Reply<T> Succeeds<T>(this Reply<T> result)
        {
            result.Success.ShouldBeTrue(result.ToString());

            return result;
        }

        private static Reply<T> WithUnparsedText<T>(this Reply<T> result, string expected)
        {
            result.UnparsedTokens.ToString().ShouldEqual(expected);

            return result;
        }

        private static Reply<T> WithAllInputConsumed<T>(this Reply<T> result)
        {
            var consumedAllInput = result.UnparsedTokens.CurrentToken.Kind == Lexer.EndOfInput;
            consumedAllInput.ShouldBeTrue("Did not consume all input.");
            result.UnparsedTokens.ToString().ShouldEqual("");

            return result;
        }

        public static Reply<T> IntoValue<T>(this Reply<T> result, T expected)
        {
            result.Value.ShouldEqual(expected);
            return result;
        }

        public static Reply<T> IntoValue<T>(this Reply<T> result, Action<T> assertParsedValue)
        {
            assertParsedValue(result.Value);
            return result;
        }

        public static Reply<Token> IntoToken(this Reply<Token> result, TokenKind expectedKind, string expectedLiteral)
        {
            result.Value.ShouldBe(expectedKind, expectedLiteral);
            return result;
        }

        public static Reply<Token> IntoToken(this Reply<Token> result, string expectedLiteral)
        {
            result.Value.Literal.ShouldEqual(expectedLiteral);
            return result;
        }

        public static Reply<IEnumerable<Token>> IntoTokens(this Reply<IEnumerable<Token>> result, params string[] expectedLiterals)
        {
            result.IntoValue(tokens => tokens.Select(x => x.Literal).ShouldList(expectedLiterals));
            return result;
        }
    }
}