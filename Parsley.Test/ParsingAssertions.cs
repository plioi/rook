using System;
using NUnit.Framework;

namespace Parsley
{
    public static class ParsingAssertions
    {
        public static void ShouldBe(this Token actual, TokenKind expectedKind, string expectedLiteral, int expectedLine, int expectedColumn)
        {
            ShouldBe(actual, expectedKind, expectedLiteral);
            actual.Position.Line.ShouldEqual(expectedLine);
            actual.Position.Column.ShouldEqual(expectedColumn);
        }

        public static void ShouldBe(this Token actual, TokenKind expectedKind, string expectedLiteral)
        {
            actual.Kind.ShouldEqual(expectedKind);
            actual.Literal.ShouldEqual(expectedLiteral);
        }

        public static Parsed<T> FailsToParse<T>(this Parser<T> parse, Lexer tokens, string expectedUnparsedSource)
        {
            return parse(tokens).Fails().WithUnparsedText(expectedUnparsedSource);
        }

        private static Parsed<T> Fails<T>(this Parsed<T> result)
        {
            result.IsError.ShouldBeTrue("Parse completed without expected error.");

            return result;
        }

        public static void WithMessage<T>(this Parsed<T> result, string expectedMessage)
        {
            result.ToString().ShouldEqual(expectedMessage);
        }

        public static Parsed<T> PartiallyParses<T>(this Parser<T> parse, Lexer tokens, string expectedUnparsedSource)
        {
            return parse(tokens).Succeeds().WithUnparsedText(expectedUnparsedSource);
        }

        public static Parsed<T> Parses<T>(this Parser<T> parse, Lexer tokens)
        {
            return parse(tokens).Succeeds().WithAllInputConsumed();
        }

        private static Parsed<T> Succeeds<T>(this Parsed<T> result)
        {
            if (result.IsError)
                Assert.Fail(result.ToString());

            return result;
        }

        private static Parsed<T> WithUnparsedText<T>(this Parsed<T> result, string expected)
        {
            result.UnparsedTokens.ToString().ShouldEqual(expected);

            return result;
        }

        private static Parsed<T> WithAllInputConsumed<T>(this Parsed<T> result)
        {
            result.UnparsedTokens.IsEndOfInput.ShouldBeTrue("Did not consume all input.");
            result.UnparsedTokens.ToString().ShouldEqual("");

            return result;
        }

        public static void IntoValue<T>(this Parsed<T> result, T expected)
        {
            result.Value.ShouldEqual(expected);
        }

        public static void IntoValue<T>(this Parsed<T> result, Action<T> assertParsedValue)
        {
            assertParsedValue(result.Value);
        }
    }
}