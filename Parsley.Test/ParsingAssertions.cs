using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Parsley
{
    public static class ParsingAssertions
    {
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
                            else
                                parsedValue.ToString().ShouldEqual(expectedValue);
                        });
        }

        public static void AssertParse<T>(this Parser<IEnumerable<T>> parse, string source, T[] expectedValues, string expectedUnparsedSource)
        {
            AssertParse(parse, source, expectedUnparsedSource,
                        parsedValues => parsedValues.ShouldList(expectedValues));
        }

        public static void AssertParse<T, U>(this Parser<Tuple<T, U>> parse, string source, T expectedFirst, U expectedSecond, string expectedUnparsedSource)
        {
            AssertParse(parse, source, expectedUnparsedSource,
                        parsedPair =>
                        {
                            parsedPair.Item1.ShouldEqual(expectedFirst);
                            parsedPair.Item2.ShouldEqual(expectedSecond);
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