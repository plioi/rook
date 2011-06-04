using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Parsley
{
    public static class ParsingAssertions
    {
        public static void AssertToken(this Token actual, object expectedKind, string expectedLiteral, int expectedLine, int expectedColumn)
        {
            AssertToken(actual, expectedKind, expectedLiteral);
            actual.Position.Line.ShouldEqual(expectedLine);
            actual.Position.Column.ShouldEqual(expectedColumn);
        }

        public static void AssertToken(this Token actual, object expectedKind, string expectedLiteral)
        {
            actual.Kind.ShouldEqual(expectedKind);
            actual.Literal.ShouldEqual(expectedLiteral);
        }

        public static void AssertError<TSyntax>(this Parser<TSyntax> parse, string source, string expectedUnparsedSource)
        {
            //Assert a parse error at the end of a single-line input with no known expectation.
            parse.AssertError(source, expectedUnparsedSource, string.Format("(1, {0}): Parse error.", source.Length + 1 - expectedUnparsedSource.Length));
        }

        public static void AssertError<TSyntax>(this Parser<TSyntax> parse, string source, string expectedUnparsedSource, string expectedMessage)
        {
            Parsed<TSyntax> result = parse(new Text(source));
            result.IsError.ShouldBeTrue("Parse completed without expected error.");
            result.UnparsedText.ToString().ShouldEqual(expectedUnparsedSource);
            result.ToString().ShouldEqual(expectedMessage);
        }

        public static void AssertParse<T>(this Parser<T> parse, string source, string expectedValue, string expectedUnparsedSource)
        {
            AssertParse(parse, source, expectedUnparsedSource,
                        parsedValue =>
                        {
                            if (expectedValue == null)
                                parsedValue.ShouldBeNull();
                            else if (parsedValue is Token)
                                (parsedValue as Token).Literal.ShouldEqual(expectedValue);
                            else
                                parsedValue.ToString().ShouldEqual(expectedValue);
                        });
        }

        public static void AssertParse(this Parser<IEnumerable<Token>> parse, string source, string[] expectedTokenLiterals, string expectedUnparsedSource)
        {
            AssertParse(parse, source, expectedUnparsedSource,
                        parsedTokens => parsedTokens.Select(x => x.Literal).ShouldList(expectedTokenLiterals));
        }

        public static void AssertParse(this Parser<Tuple<Token, Token>> parse, string source, string expectedFirst, string expectedSecond, string expectedUnparsedSource)
        {
            AssertParse(parse, source, expectedUnparsedSource,
                        parsedPair =>
                        {
                            parsedPair.Item1.Literal.ShouldEqual(expectedFirst);
                            parsedPair.Item2.Literal.ShouldEqual(expectedSecond);
                        });
        }

        public static void AssertParse<T>(this Parser<T> parse, string source, string expectedUnparsedSource, Action<T> assertParsedValue)
        {
            Parsed<T> result = parse(new Text(source));
            AssertParseSucceeded(result);
            AssertUnparsedSource(result, expectedUnparsedSource);
            assertParsedValue(result.Value);
        }

        private static void AssertParseSucceeded<T>(Parsed<T> result)
        {
            if (result.IsError)
                Assert.Fail(result.ToString());
        }

        private static void AssertUnparsedSource<T>(Parsed<T> result, string expectedUnparsedSource) 
        {
            if (expectedUnparsedSource == "")
                result.UnparsedText.EndOfInput.ShouldBeTrue("Did not consume all input.");
            result.UnparsedText.ToString().ShouldEqual(expectedUnparsedSource);
        }
    }
}