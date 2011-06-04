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

            parser.PartiallyParses("input", "input").IntoValue(1);
        }

        [Test]
        public void CanBuildParserFromSingleSimplerParser()
        {
            var parser = from x in Char('x')
                         select x.ToUpper();
            
            parser.PartiallyParses("xy", "y").IntoValue("X");
        }

        [Test]
        public void CanBuildParserFromOrderedSequenceOfSimplerParsers()
        {
            var parser = (from a in Char('a')
                          from b in Char('b')
                          from c in Char('c')
                          select (a + b + c).ToUpper());

            parser.PartiallyParses("abcdef", "def").IntoValue("ABC");
        }

        [Test]
        public void PropogatesErrorsWithoutRunningRemainingParsers()
        {
            (from _ in Fail
             from x in Char('x')
             from y in Char('y')
             select Tuple.Create(x, y)).FailsToParse("xy", "xy");

            (from x in Char('x')
             from _ in Fail
             from y in Char('y')
             select Tuple.Create(x, y)).FailsToParse("xy", "y");

            (from x in Char('x')
             from y in Char('y')
             from _ in Fail
             select Tuple.Create(x, y)).FailsToParse("xy", "");
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