using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public sealed class AbstractGrammarSpec : AbstractGrammar
    {
        private static Action<Token> Token(string expectedLiteral)
        {
            return token => token.Literal.ShouldEqual(expectedLiteral);
        }

        private static Action<IEnumerable<Token>> Tokens(params string[] expectedLiterals)
        {
            return tokens => tokens.Select(x => x.Literal).ShouldList(expectedLiterals);
        }

        [Test]
        public void CanDetectTheEndOfInputWithoutAdvancing()
        {
            EndOfInput.Parses("").IntoValue("");
            EndOfInput.FailsToParse("!", "!");
        }

        [Test]
        public void ApplyingARuleFollowedByARequiredButDiscardedTerminatorRule()
        {
            Parser<Token> terminatedGoal = String("(Goal)").TerminatedBy(String("(Terminator)"));

            terminatedGoal.PartiallyParses("(Goal)(Terminator)b", "b").IntoValue(Token("(Goal)"));
            terminatedGoal.FailsToParse("", "");
            terminatedGoal.FailsToParse("!", "!");
            terminatedGoal.FailsToParse("(Goal)!", "!");
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimes()
        {
            var parser = ZeroOrMore(Digit());

            parser.Parses("").IntoValue(Tokens());
            parser.PartiallyParses("!", "!").IntoValue(Tokens());

            parser.PartiallyParses("0!", "!").IntoValue(Tokens("0"));
            parser.PartiallyParses("01!", "!").IntoValue(Tokens("0", "1"));
            parser.PartiallyParses("012!", "!").IntoValue(Tokens("0", "1", "2"));

            parser.Parses("0").IntoValue(Tokens("0"));
            parser.Parses("01").IntoValue(Tokens("0", "1"));
            parser.Parses("012").IntoValue(Tokens("0", "1", "2"));
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimes()
        {
            var parser = OneOrMore(Digit());

            parser.FailsToParse("", "");
            parser.FailsToParse("!", "!");

            parser.PartiallyParses("0!", "!").IntoValue(Tokens("0"));
            parser.PartiallyParses("01!", "!").IntoValue(Tokens("0", "1"));
            parser.PartiallyParses("012!", "!").IntoValue(Tokens("0", "1", "2"));

            parser.Parses("0").IntoValue(Tokens("0"));
            parser.Parses("01").IntoValue(Tokens("0", "1"));
            parser.Parses("012").IntoValue(Tokens("0", "1", "2"));
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimesInterspersedByASeparatorRule()
        {
            var parser = ZeroOrMore(Digit(), String(","));

            parser.Parses("").IntoValue(Tokens());
            parser.PartiallyParses("!", "!").IntoValue(Tokens());

            parser.PartiallyParses("0!", "!").IntoValue(Tokens("0"));
            parser.PartiallyParses("0,1!", "!").IntoValue(Tokens("0", "1"));
            parser.PartiallyParses("0,1,2!", "!").IntoValue(Tokens("0", "1", "2"));

            parser.Parses("0").IntoValue(Tokens("0"));
            parser.Parses("0,1").IntoValue(Tokens("0", "1"));
            parser.Parses("0,1,2").IntoValue(Tokens("0", "1", "2"));
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimesFollowedByARequiredTerminatorRule()
        {
            var parser = ZeroOrMoreTerminated(Digit(), String("EndOfDigits"));

            parser.FailsToParse("", "");
            parser.FailsToParse("MissingTerminator", "MissingTerminator");
            parser.FailsToParse("0MissingTerminator", "MissingTerminator");
            parser.FailsToParse("01MissingTerminator", "MissingTerminator");
            parser.FailsToParse("012MissingTerminator", "MissingTerminator");

            parser.Parses("EndOfDigits").IntoValue(Tokens());
            parser.PartiallyParses("EndOfDigits!", "!").IntoValue(Tokens());
            parser.Parses("0EndOfDigits").IntoValue(Tokens("0"));
            parser.Parses("01EndOfDigits").IntoValue(Tokens("0", "1"));
            parser.PartiallyParses("012EndOfDigits!", "!").IntoValue(Tokens("0", "1", "2"));
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
        {
            var parser = OneOrMore(Digit(), String(","));

            parser.FailsToParse("", "");
            parser.FailsToParse("!", "!");

            parser.PartiallyParses("0!", "!").IntoValue(Tokens("0"));
            parser.PartiallyParses("0,1!", "!").IntoValue(Tokens("0", "1"));
            parser.PartiallyParses("0,1,2!", "!").IntoValue(Tokens("0", "1", "2" ));

            parser.Parses("0").IntoValue(Tokens("0"));
            parser.Parses("0,1").IntoValue(Tokens("0", "1"));
            parser.Parses("0,1,2").IntoValue(Tokens("0", "1", "2"));
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimesInterspersedByALeftAssociativeSeparatorRule()
        {
            var parser =
                LeftAssociative(
                    Digit(),
                    Pattern(@"[\*/]"),
                    (left, symbolAndRight) =>
                    new Token(null, symbolAndRight.Item1.Position, System.String.Format("({0} {1} {2})", symbolAndRight.Item1.Literal, left.Literal, symbolAndRight.Item2.Literal)));

            parser.FailsToParse("!", "!");
            parser.Parses("0").IntoValue(Token("0"));
            parser.Parses("0*1").IntoValue(Token("(* 0 1)"));
            parser.Parses("0*1/2").IntoValue(Token("(/ (* 0 1) 2)"));
        }

        [Test]
        public void ApplyingAPairOfOrderedRules()
        {
            var parser = Pair(String("0"), String("1"));

            parser.FailsToParse("10", "10");

            parser.Parses("01").IntoValue(pair =>
            {
                pair.Item1.Literal.ShouldEqual("0");
                pair.Item2.Literal.ShouldEqual("1");
            });
        }

        [Test]
        public void ApplyingARuleBetweenTwoOtherRules()
        {
            Parser<Token> surroundedGoal = Between(String("(Left)"), String("(Goal)"), String("(Right)"));

            surroundedGoal.PartiallyParses("(Left)(Goal)(Right)b", "b").IntoValue(Token("(Goal)"));
            surroundedGoal.FailsToParse("(Left)", "");
            surroundedGoal.FailsToParse("(Left)!", "!");
            surroundedGoal.FailsToParse("(Left)(Goal)!", "!");
        }

        [Test]
        public void ApplyingANegativeLookaheadAssertionWithoutConsumingInput()
        {
            Parser<Token> notX = Not(String("x"));
            notX.PartiallyParses("y", "y").IntoValue(value => value.ShouldBeNull());
            notX.FailsToParse("x", "x");
        }

        [Test]
        public void ParsingAnOptionalRuleZeroOrOneTimes()
        {
            Parser<Token> optionalCash = Optional(String("$"));
            optionalCash.PartiallyParses("$.", ".").IntoValue(Token("$"));
            optionalCash.PartiallyParses(".", ".").IntoValue(token => token.ShouldBeNull());
        }

        [Test]
        public void ParsingARuleOnlyWhenItsResultWouldPassesAPredicate()
        {
            Predicate<Token> isDollars = x => x.Literal == "$";
            Predicate<Token> isCents = x => x.Literal == "¢";

            Parser<Token> cash = Pattern(@"[\$¢]");

            Expect(cash, isDollars).FailsToParse("!", "!");
            Expect(cash, isCents).FailsToParse("!", "!");

            Expect(cash, isDollars).Parses("$").IntoValue(Token("$"));
            Expect(cash, isCents).Parses("¢").IntoValue(Token("¢"));

            Expect(cash, isDollars).FailsToParse("¢", "¢");
            Expect(cash, isCents).FailsToParse("$", "$");
        }

        [Test]
        public void ChoosingTheFirstSuccessfulParserFromAPrioritizedList()
        {
            Parser<Token> parenthesizedA = Between(String("("), String("a"), String(")"));
            Parser<Token> parenthesizedAB = Between(String("("), String("ab"), String(")"));
            Parser<Token> parenthesizedABC = Between(String("("), String("abc"), String(")"));

            Parser<Token> choice2 = Choice(
                OnError(parenthesizedA, "parenthesized a"),
                OnError(parenthesizedAB, "parenthesized ab"),
                OnError(parenthesizedABC, "parenthesized abc"));
            
            choice2.PartiallyParses("(a)bcd", "bcd").IntoValue(Token("a")); //First rule wins.
            choice2.PartiallyParses("(ab)cd", "cd").IntoValue(Token("ab")); //Second rule wins.
            choice2.PartiallyParses("(abc)d", "d").IntoValue(Token("abc")); //Third rule wins.

            //When all rules fail, the error returned should correspond with the
            //rule that made it deepest into the input before encountering a failure.
            choice2.FailsToParse("(a!", "!").WithMessage("(1, 3): parenthesized a expected"); //First rule's error wins.
            choice2.FailsToParse("(ab!", "!").WithMessage("(1, 4): parenthesized ab expected"); //Second rule's error wins.
            choice2.FailsToParse("(abc!", "!").WithMessage("(1, 5): parenthesized abc expected"); //Third rule's error wins.

            //When all rules fail, and there is a tie while selecting the rule that 
            //made it deepest into the input, favor the rules in the order they were
            //declared.
            choice2.FailsToParse("(x", "x").WithMessage("(1, 2): parenthesized a expected"); //First rule's error wins.
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
            Parser<Token> x = String("x");
            Parser<Token> xImproved = OnError(x, "letter x");
            x.FailsToParse("y", "y").WithMessage("(1, 1): Parse error.");
            xImproved.FailsToParse("y", "y").WithMessage("(1, 1): letter x expected");
        }

        [Test]
        public void ProvidingTheCurrentPositionWithoutConsumingInput()
        {
            Parser<Position> start = Position;
            Parser<Position> afterLeadingWhiteSpace = Between(Pattern(@"\s+"), Position, Pattern(@"[0-9]+"));

            start.PartiallyParses("ABC", "ABC").IntoValue(position =>
            {
                position.Line.ShouldEqual(1);
                position.Column.ShouldEqual(1);
            });

            afterLeadingWhiteSpace.PartiallyParses("  \r\n   \r\n   123!", "!").IntoValue(position =>
            {
                position.Line.ShouldEqual(3);
                position.Column.ShouldEqual(4);
            });
        }

        private static Parser<Token> String(string literal)
        {
            return Pattern(Regex.Escape(literal));
        }

        private static Parser<Token> Digit()
        {
            return Pattern(@"[0-9]");
        }

        private static Parser<Token> Pattern(string pattern)
        {
            return text =>
            {
                Lexer lexer = new StubLexer(text, pattern);

                if (lexer.CurrentToken.Kind == null)
                    return new Success<Token>(lexer.CurrentToken, lexer.Advance().Text);

                return new Error<Token>(text);
            };
        }

        private class StubLexer : Lexer
        {
            public StubLexer(Text text, string pattern)
                : base(text, new TokenMatcher(null, pattern)) { }
        }
    }
}