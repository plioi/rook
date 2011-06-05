using System;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class ParserQuerySpec
    {
        private static Lexer Tokenize(string source)
        {
            return new CharLexer(source);
        }

        [Test]
        public void CanBuildParserWhichSimulatesSuccessfulParsingOfGivenValueWithoutConsumingInput()
        {
            var parser = 1.SucceedWithThisValue();

            parser.PartiallyParses(Tokenize("input"), "input").IntoValue(1);
        }

        [Test]
        public void CanBuildParserFromSingleSimplerParser()
        {
            var parser = from x in Char('x')
                         select x.ToUpper();

            parser.PartiallyParses(Tokenize("xy"), "y").IntoValue("X");
        }

        [Test]
        public void CanBuildParserFromOrderedSequenceOfSimplerParsers()
        {
            var parser = (from a in Char('a')
                          from b in Char('b')
                          from c in Char('c')
                          select (a + b + c).ToUpper());

            parser.PartiallyParses(Tokenize("abcdef"), "def").IntoValue("ABC");
        }

        [Test]
        public void PropogatesErrorsWithoutRunningRemainingParsers()
        {
            var source = Tokenize("xy");

            (from _ in Fail
             from x in Char('x')
             from y in Char('y')
             select Tuple.Create(x, y)).FailsToParse(source, "xy");

            (from x in Char('x')
             from _ in Fail
             from y in Char('y')
             select Tuple.Create(x, y)).FailsToParse(source, "y");

            (from x in Char('x')
             from y in Char('y')
             from _ in Fail
             select Tuple.Create(x, y)).FailsToParse(source, "");
        }

        private static Parser<string> Char(char letter)
        {
            return tokens =>
            {
                if (tokens.CurrentToken.Literal == letter.ToString())
                    return new Success<string>(tokens.CurrentToken.Literal, tokens.Advance());

                return new Error<string>(tokens);
            };
        }

        private static readonly Parser<string> Fail = tokens => new Error<string>(tokens);
    }
}