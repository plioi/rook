using System;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class ParserQuerySpec
    {
        [Test]
        public void CanBuildParserWhichSimulatesSuccessfulParsingOfGivenValueWithoutConsumingInput()
        {
            var parser = 1.SucceedWithThisValue();

            parser.AssertParse("input", "1", "input");
        }

        [Test]
        public void CanBuildParserFromSingleSimplerParser()
        {
            (from x in Char('x')
             select x.ToUpper()).AssertParse("xy", "X", "y");
        }

        [Test]
        public void CanBuildParserFromOrderedSequenceOfSimplerParsers()
        {
            (from a in Char('a')
             from b in Char('b')
             from c in Char('c')
             select (a + b + c).ToUpper()).AssertParse("abcdef", "ABC", "def");
        }

        [Test]
        public void PropogatesErrorsWithoutRunningRemainingParsers()
        {
            (from _ in Fail
             from x in Char('x')
             from y in Char('y')
             select Tuple.Create(x, y)).AssertError("xy", "xy");

            (from x in Char('x')
             from _ in Fail
             from y in Char('y')
             select Tuple.Create(x, y)).AssertError("xy", "y");

            (from x in Char('x')
             from y in Char('y')
             from _ in Fail
             select Tuple.Create(x, y)).AssertError("xy", "");
        }

        private static Parser<string> Char(char letter)
        {
            return text =>
            {
                if (text.Peek(1) == letter.ToString())
                    return new Success<string>(text.Peek(1), text.Advance(1));

                return new Error<string>(text);
            };
        }

        private static readonly Parser<string> Fail = text => new Error<string>(text);
    }
}