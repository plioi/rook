using System;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class AbstractGrammarSpec : AbstractGrammar
    {
        [Test]
        public void CanParseASingleExpectedString()
        {
            String("ABC").AssertParse("ABC", "ABC", "");
            String("AB").AssertParse("ABC", "AB", "C");
            String("").AssertParse("A", "", "A");

            String("ABC").AssertError("AB", "AB");
            String("BC").AssertError("ABC", "ABC");
            String("A").AssertError("", "");
        }

        [Test]
        public void CanParseASingleStringFromAPrioritizedSetOfPossibleMatches()
        {
            String("ABC", "XYZ").AssertParse("ABC", "ABC", "");
            String("XYZ", "ABC").AssertParse("ABC", "ABC", "");

            String("AB", "XY").AssertParse("ABC", "AB", "C");
            String("XY", "AB").AssertParse("ABC", "AB", "C");

            String("", "A").AssertParse("A", "", "A");
            String("A", "").AssertParse("A", "A", "");

            String("ABC", "XYZ").AssertError("AB", "AB");
            String("BC", "XYZ").AssertError("ABC", "ABC");
            String("A", "XYZ").AssertError("", "");
        }

        [Test]
        public void CanParseCharactersSatisfyingARegex()
        {
            Pattern(@"[a-zA-Z]+").AssertParse("ABc", "ABc", "");
            Pattern(@"[A-Z]+").AssertParse("ABc", "AB", "c");
            Pattern(@"[0-9]+").AssertError("ABc", "ABc");
            Pattern(@"[0-9]").AssertError("", "");
        }

        [Test]
        public void CanDetectTheEndOfInputWithoutAdvancing()
        {
            EndOfInput.AssertParse("", "", "");
            EndOfInput.AssertError("!", "!");
        }

        [Test]
        public void ApplyingARuleFollowedByARequiredButDiscardedTerminatorRule()
        {
            Parser<string> terminatedGoal = String("(Goal)").TerminatedBy(String("(Terminator)"));

            terminatedGoal.AssertParse("(Goal)(Terminator)b", "(Goal)", "b");
            terminatedGoal.AssertError("", "");
            terminatedGoal.AssertError("!", "!");
            terminatedGoal.AssertError("(Goal)!", "!");
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimes()
        {
            var parse = ZeroOrMore(String("0", "1", "2"));

            parse.AssertParse("", new string[] {}, "");
            parse.AssertParse("!", new string[] {}, "!");

            parse.AssertParse("0!", new[] {"0"}, "!");
            parse.AssertParse("01!", new[] {"0", "1"}, "!");
            parse.AssertParse("012!", new[] {"0", "1", "2"}, "!");

            parse.AssertParse("0", new[] { "0" }, "");
            parse.AssertParse("01", new[] { "0", "1" }, "");
            parse.AssertParse("012", new[] { "0", "1", "2" }, "");
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimes()
        {
            var parse = OneOrMore(String("0", "1", "2"));

            parse.AssertError("", "");
            parse.AssertError("!", "!");

            parse.AssertParse("0!", new[] {"0"}, "!");
            parse.AssertParse("01!", new[] {"0", "1"}, "!");
            parse.AssertParse("012!", new[] {"0", "1", "2"}, "!");

            parse.AssertParse("0", new[] { "0" }, "");
            parse.AssertParse("01", new[] { "0", "1" }, "");
            parse.AssertParse("012", new[] { "0", "1", "2" }, "");
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimesInterspersedByASeparatorRule()
        {
            var parse = ZeroOrMore(String("0", "1", "2"), String(","));

            parse.AssertParse("", new string[] {}, "");
            parse.AssertParse("!", new string[] {}, "!");

            parse.AssertParse("0!", new[] {"0"}, "!");
            parse.AssertParse("0,1!", new[] {"0", "1"}, "!");
            parse.AssertParse("0,1,2!", new[] {"0", "1", "2"}, "!");

            parse.AssertParse("0", new[] { "0" }, "");
            parse.AssertParse("0,1", new[] { "0", "1" }, "");
            parse.AssertParse("0,1,2", new[] { "0", "1", "2" }, "");
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimesFollowedByARequiredTerminatorRule()
        {
            var parse = ZeroOrMoreTerminated(String("0", "1", "2"), String("EndOfDigits"));

            parse.AssertError("", "");
            parse.AssertError("MissingTerminator", "MissingTerminator");
            parse.AssertError("0MissingTerminator", "MissingTerminator");
            parse.AssertError("01MissingTerminator", "MissingTerminator");
            parse.AssertError("012MissingTerminator", "MissingTerminator");

            parse.AssertParse("EndOfDigits", new string[] { }, "");
            parse.AssertParse("EndOfDigits!", new string[] { }, "!");
            parse.AssertParse("0EndOfDigits", new[] { "0" }, "");
            parse.AssertParse("01EndOfDigits", new[] { "0", "1" }, "");
            parse.AssertParse("012EndOfDigits!", new[] { "0", "1", "2" }, "!");
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
        {
            var parse = OneOrMore(String("0", "1", "2"), String(","));

            parse.AssertError("", "");
            parse.AssertError("!", "!");

            parse.AssertParse("0!", new[] { "0" }, "!");
            parse.AssertParse("0,1!", new[] { "0", "1" }, "!");
            parse.AssertParse("0,1,2!", new[] { "0", "1", "2" }, "!");

            parse.AssertParse("0", new[] { "0" }, "");
            parse.AssertParse("0,1", new[] { "0", "1" }, "");
            parse.AssertParse("0,1,2", new[] { "0", "1", "2" }, "");
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimesInterspersedByALeftAssociativeSeparatorRule()
        {
            var parse =
                LeftAssociative(
                    String("0", "1", "2"),
                    String("*", "/"),
                    (left, symbolAndRight) =>
                    System.String.Format("({0} {1} {2})", symbolAndRight.Item1, left, symbolAndRight.Item2));

            parse.AssertError("!", "!");
            parse.AssertParse("0", "0", "");
            parse.AssertParse("0*1", "(* 0 1)", "");
            parse.AssertParse("0*1/2", "(/ (* 0 1) 2)", "");
        }

        [Test]
        public void ApplyingAPairOfOrderedRules()
        {
            var parse = Pair(String("0"), String("1"));

            parse.AssertError("10", "10");
            parse.AssertParse("01", "0", "1", "");
        }

        [Test]
        public void ApplyingARuleBetweenTwoOtherRules()
        {
            Parser<string> surroundedGoal = Between(String("(Left)"), String("(Goal)"), String("(Right)"));

            surroundedGoal.AssertParse("(Left)(Goal)(Right)b", "(Goal)", "b");
            surroundedGoal.AssertError("(Left)", "");
            surroundedGoal.AssertError("(Left)!", "!");
            surroundedGoal.AssertError("(Left)(Goal)!", "!");
        }

        [Test]
        public void ApplyingANegativeLookaheadAssertionWithoutConsumingInput()
        {
            Parser<string> notX = Not(String("x"));
            notX.AssertParse("y", null, "y");
            notX.AssertError("x", "x");
        }

        [Test]
        public void ParsingAnOptionalRuleZeroOrOneTimes()
        {
            Parser<string> optionalCash = Optional(String("$"));
            optionalCash.AssertParse("$.", "$", ".");
            optionalCash.AssertParse(".", null, ".");
        }

        [Test]
        public void ParsingARuleOnlyWhenItsResultWouldPassesAPredicate()
        {
            Predicate<string> isDollars = x => x == "$";
            Predicate<string> isCents = x => x == "¢";

            Parser<string> cash = String("$", "¢");

            Expect(cash, isDollars).AssertError("!", "!");
            Expect(cash, isCents).AssertError("!", "!");

            Expect(cash, isDollars).AssertParse("$", "$", "");
            Expect(cash, isCents).AssertParse("¢", "¢", "");

            Expect(cash, isDollars).AssertError("¢", "¢");
            Expect(cash, isCents).AssertError("$", "$");
        }

        [Test]
        public void ChoosingTheFirstSuccessfulParserFromAPrioritizedList()
        {
            Parser<string> parenthesizedA = Between(String("("), String("a"), String(")"));
            Parser<string> parenthesizedAB = Between(String("("), String("ab"), String(")"));
            Parser<string> parenthesizedABC = Between(String("("), String("abc"), String(")"));
            
            Parser<string> choice2 = Choice(
                OnError(parenthesizedA, "parenthesized a"),
                OnError(parenthesizedAB, "parenthesized ab"),
                OnError(parenthesizedABC, "parenthesized abc"));
            
            choice2.AssertParse("(a)bcd", "a", "bcd"); //First rule wins.
            choice2.AssertParse("(ab)cd", "ab", "cd"); //Second rule wins.
            choice2.AssertParse("(abc)d", "abc", "d"); //Third rule wins.

            //When all rules fail, the error returned should correspond with the
            //rule that made it deepest into the input before encountering a failure.
            choice2.AssertError("(a!", "!", "(1, 3): parenthesized a expected"); //First rule's error wins.
            choice2.AssertError("(ab!", "!", "(1, 4): parenthesized ab expected"); //Second rule's error wins.
            choice2.AssertError("(abc!", "!", "(1, 5): parenthesized abc expected"); //Third rule's error wins.

            //When all rules fail, and there is a tie while selecting the rule that 
            //made it deepest into the input, favor the rules in the order they were
            //declared.
            choice2.AssertError("(x", "x", "(1, 2): parenthesized a expected"); //First rule's error wins.
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Missing choice.")]
        public void DemandsAtLeastOneChoiceWhenBuidingAChoiceParser()
        {
            Choice(new Parser<string>[] {});
        }

        [Test]
        public void ImprovingDefaultErrorMessagesWithAKnownExpectation()
        {
            Parser<string> x = String("x");
            Parser<string> xImproved = OnError(x, "letter x");
            x.AssertError("y", "y");
            xImproved.AssertError("y", "y", "(1, 1): letter x expected");
        }

        [Test]
        public void ProvidingTheCurrentPositionWithoutConsumingInput()
        {
            Parser<Position> start = Position;
            Parser<Position> afterLeadingWhiteSpace = Between(Pattern(@"\s+"), Position, Pattern(@"[0-9]+"));

            start.AssertParse("ABC", "ABC", position =>
            {
                position.Line.ShouldEqual(1);
                position.Column.ShouldEqual(1);
            });

            afterLeadingWhiteSpace.AssertParse("  \r\n   \r\n   123!", "!", position =>
            {
                position.Line.ShouldEqual(3);
                position.Column.ShouldEqual(4);
            });
        }
    }
}