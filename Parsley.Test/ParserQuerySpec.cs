using System;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class ParserQuerySpec
    {
        [Test(Description = "Bind(Unit(value), elevator) = elevator(value)")]
        public void SatisfiesTheLeftIdentityLaw()
        {
            Parser<string> left = Bind(Unit(""), s => Char('a'));
            Parser<string> right = Char('a');
            left.AssertParse("a0", "a", "0");
            right.AssertParse("a0", "a", "0");
        }

        [Test(Description = "Bind(elevated, Unit) = elevated")]
        public void SatisfiesTheRightIdentityLaw()
        {
            Parser<string> left = Bind(Char('a'), Unit);
            Parser<string> right = Char('a');
            left.AssertParse("a0", "a", "0");
            right.AssertParse("a0", "a", "0");
        }

        [Test(Description = "Bind(elevated, x => Bind(k(x), y => h(y)) = Bind(Bind(elevated, x => k(x)), y => h(y))")]
        public void SatisfiesTheAssociativeLaw()
        {
            //'x' >>= ('y' >>= 'z') == ('x' >>= 'y') >>= 'z'
            Parser<string> left = Bind(Char('x'), x => Bind(Char('y'), y => Char('z')));
            Parser<string> right = Bind(Bind(Char('x'), x => Char('y')), y => Char('z'));
            left.AssertParse("xyz!", "z", "!");
            right.AssertParse("xyz!", "z", "!");
        }

        [Test]
        public void SupportsLinqSyntaxForBuildingParsersFromOtherParsers()
        {
            //Bind notation.
            (Bind(Char('x'), x =>
                Bind(Char('y'), y =>
                    ((x+y).ToUpper()).ToParser()))).AssertParse("xy", "XY", "");

            //Bind is in fact SelectMany.
            (Char('x').SelectMany(x =>
                Char('y').SelectMany(y =>
                    ((x+y).ToUpper()).ToParser()))).AssertParse("xy", "XY", "");

            //LINQ notation hides nested calls to SelectMany.
            //Each 'from' introduces the ordered parsers to 
            //combine into the result parser.  The result of 
            //a query is a function that is a Parser<T>.
            (from x in Char('x')
             from y in Char('y')
             select ((x+y).ToUpper())).AssertParse("xy", "XY", "");
        }

        [Test]
        public void SupportsLinqSyntaxForBuildingAParserFromASingleSimplerParser()
        {
            (from x in Char('x')
             select x.ToUpper()).AssertParse("xy", "X", "y");
        }

        [Test]
        public void WalksThroughInputOneParserAtATimeWhileCollectingIntermediateResults()
        {
            (from a in Char('a')
             from b in Char('b')
             from c in Char('c')
             select (a + b + c).ToUpper()).AssertParse("abcdef", "ABC", "def");
        }

        [Test]
        public void PropogatesErrorsWithoutConsumingMoreText()
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

        [Test]
        public void PropogatesErrorsWithoutRunningMoreParsers()
        {
            //Parsers should never throw exceptions in practice.  The 'shouldNotBeCalled' parser is 
            //defined like this to demonstrate that it isn't even executed by the query.  Since a 
            //legitimate parse error is encountered first (via Fail), the remaining parsers in the query are
            //skipped over in order to propogate the error.

            Parser<string> shouldNotBeCalled = text => { throw new Exception(); };

            (from x in Char('x')
             from fail in Fail
             from neverAssignedValue in shouldNotBeCalled
             select neverAssignedValue).AssertError("xy", "y");

            (from x in Char('x')
             from fail in OnError(Fail, "something")
             from neverAssignedValue in shouldNotBeCalled
             select neverAssignedValue).AssertError("xy", "y", "(1, 2): something expected");
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

        private static Parser<T> OnError<T>(Parser<T> parse, string expectation)
        {
            return AbstractGrammar.OnError(parse, expectation);
        }

        private static Parser<T> Unit<T>(T value)
        {
            return value.ToParser();
        }

        private static Parser<TOutput> Bind<TInput, TOutput>(Parser<TInput> elevated, Func<TInput, Parser<TOutput>> k)
        {
            return elevated.SelectMany(k);
        }
    }
}