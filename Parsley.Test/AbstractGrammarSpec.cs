using System;
using System.Collections.Generic;
using System.Linq;
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

        private static Lexer Tokenize(string source)
        {
            return new SampleLexer(source);
        }

        private static Parser<Token> DIGIT { get { return Kind(SampleLexer.Digit); } }
        private static Parser<Token> LETTER { get { return Kind(SampleLexer.Letter); } }
        private static Parser<Token> COMMA { get { return Kind(SampleLexer.Comma); } }
        private static Parser<Token> WHITESPACE { get { return Kind(SampleLexer.WhiteSpace); } }
        private static Parser<Token> SYMBOL { get { return Kind(SampleLexer.Symbol); } }

        private sealed class SampleLexer : Lexer
        {
            public static readonly TokenKind Digit = new TokenKind(@"[0-9]");
            public static readonly TokenKind Letter = new TokenKind(@"[a-zA-Z]");
            public static readonly TokenKind Comma = new TokenKind(@"\,");
            public static readonly TokenKind WhiteSpace = new TokenKind(@"\s+");
            public static readonly TokenKind Symbol = new TokenKind(@".");

            public SampleLexer(string source)
                : base(new Text(source), Digit, Letter, Comma, WhiteSpace, Symbol) { }
        }

        [Test]
        public void CanDetectTheEndOfInputWithoutAdvancing()
        {
            EndOfInput.Parses(Tokenize("")).IntoValue("");
            EndOfInput.FailsToParse(Tokenize("!"), "!");
        }

        [Test]
        public void CanDemandThatAGivenKindOfTokenAppearsNext()
        {
            Kind(SampleLexer.Letter).Parses(Tokenize("A")).IntoValue(Token("A"));
            Kind(SampleLexer.Letter).FailsToParse(Tokenize("0"), "0");

            Kind(SampleLexer.Digit).FailsToParse(Tokenize("A"), "A");
            Kind(SampleLexer.Digit).Parses(Tokenize("0")).IntoValue(Token("0"));
        }

        [Test]
        public void ApplyingARuleFollowedByARequiredButDiscardedTerminatorRule()
        {
            Parser<Token> parser = LETTER.TerminatedBy(SYMBOL);

            parser.PartiallyParses(Tokenize("A~Unparsed"), "Unparsed").IntoValue(Token("A"));
            parser.FailsToParse(Tokenize(""), "");
            parser.FailsToParse(Tokenize("~"), "~");
            parser.FailsToParse(Tokenize("A0"), "0");
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimes()
        {
            var parser = ZeroOrMore(DIGIT);

            parser.Parses(Tokenize("")).IntoValue(Tokens());
            parser.PartiallyParses(Tokenize("!"), "!").IntoValue(Tokens());

            parser.PartiallyParses(Tokenize("0!"), "!").IntoValue(Tokens("0"));
            parser.PartiallyParses(Tokenize("01!"), "!").IntoValue(Tokens("0", "1"));
            parser.PartiallyParses(Tokenize("012!"), "!").IntoValue(Tokens("0", "1", "2"));

            parser.Parses(Tokenize("0")).IntoValue(Tokens("0"));
            parser.Parses(Tokenize("01")).IntoValue(Tokens("0", "1"));
            parser.Parses(Tokenize("012")).IntoValue(Tokens("0", "1", "2"));
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimes()
        {
            var parser = OneOrMore(DIGIT);

            parser.FailsToParse(Tokenize(""), "");
            parser.FailsToParse(Tokenize("!"), "!");

            parser.PartiallyParses(Tokenize("0!"), "!").IntoValue(Tokens("0"));
            parser.PartiallyParses(Tokenize("01!"), "!").IntoValue(Tokens("0", "1"));
            parser.PartiallyParses(Tokenize("012!"), "!").IntoValue(Tokens("0", "1", "2"));

            parser.Parses(Tokenize("0")).IntoValue(Tokens("0"));
            parser.Parses(Tokenize("01")).IntoValue(Tokens("0", "1"));
            parser.Parses(Tokenize("012")).IntoValue(Tokens("0", "1", "2"));
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimesInterspersedByASeparatorRule()
        {
            var parser = ZeroOrMore(DIGIT, COMMA);

            parser.Parses(Tokenize("")).IntoValue(Tokens());
            parser.PartiallyParses(Tokenize("!"), "!").IntoValue(Tokens());

            parser.PartiallyParses(Tokenize("0!"), "!").IntoValue(Tokens("0"));
            parser.PartiallyParses(Tokenize("0,1!"), "!").IntoValue(Tokens("0", "1"));
            parser.PartiallyParses(Tokenize("0,1,2!"), "!").IntoValue(Tokens("0", "1", "2"));

            parser.Parses(Tokenize("0")).IntoValue(Tokens("0"));
            parser.Parses(Tokenize("0,1")).IntoValue(Tokens("0", "1"));
            parser.Parses(Tokenize("0,1,2")).IntoValue(Tokens("0", "1", "2"));
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimesFollowedByARequiredTerminatorRule()
        {
            var parser = ZeroOrMoreTerminated(DIGIT, SYMBOL);

            parser.FailsToParse(Tokenize(""), "");
            parser.FailsToParse(Tokenize("MissingTerminator"), "MissingTerminator");
            parser.FailsToParse(Tokenize("0MissingTerminator"), "MissingTerminator");
            parser.FailsToParse(Tokenize("01MissingTerminator"), "MissingTerminator");

            parser.Parses(Tokenize("~")).IntoValue(Tokens());
            parser.PartiallyParses(Tokenize("~!"), "!").IntoValue(Tokens());
            parser.Parses(Tokenize("0~")).IntoValue(Tokens("0"));
            parser.Parses(Tokenize("01~")).IntoValue(Tokens("0", "1"));
            parser.PartiallyParses(Tokenize("012~!"), "!").IntoValue(Tokens("0", "1", "2"));
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
        {
            var parser = OneOrMore(DIGIT, COMMA);

            parser.FailsToParse(Tokenize(""), "");
            parser.FailsToParse(Tokenize("!"), "!");

            parser.PartiallyParses(Tokenize("0!"), "!").IntoValue(Tokens("0"));
            parser.PartiallyParses(Tokenize("0,1!"), "!").IntoValue(Tokens("0", "1"));
            parser.PartiallyParses(Tokenize("0,1,2!"), "!").IntoValue(Tokens("0", "1", "2" ));

            parser.Parses(Tokenize("0")).IntoValue(Tokens("0"));
            parser.Parses(Tokenize("0,1")).IntoValue(Tokens("0", "1"));
            parser.Parses(Tokenize("0,1,2")).IntoValue(Tokens("0", "1", "2"));
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimesInterspersedByALeftAssociativeSeparatorRule()
        {
            var parser =
                LeftAssociative(DIGIT, SYMBOL, (left, symbolAndRight) =>
                    new Token(null, symbolAndRight.Item1.Position, String.Format("({0} {1} {2})", symbolAndRight.Item1.Literal, left.Literal, symbolAndRight.Item2.Literal)));

            parser.FailsToParse(Tokenize("!"), "!");
            parser.Parses(Tokenize("0")).IntoValue(Token("0"));
            parser.Parses(Tokenize("0*1")).IntoValue(Token("(* 0 1)"));
            parser.Parses(Tokenize("0*1/2")).IntoValue(Token("(/ (* 0 1) 2)"));
        }

        [Test]
        public void ApplyingAPairOfOrderedRules()
        {
            var parser = Pair(DIGIT, LETTER);

            parser.FailsToParse(Tokenize("A0"), "A0");
            parser.Parses(Tokenize("0A")).IntoValue(pair =>
            {
                pair.Item1.Literal.ShouldEqual("0");
                pair.Item2.Literal.ShouldEqual("A");
            });
        }

        [Test]
        public void ApplyingARuleBetweenTwoOtherRules()
        {
            var parser = Between(SYMBOL, DIGIT, SYMBOL);

            parser.PartiallyParses(Tokenize("(1)Unparsed"), "Unparsed").IntoValue(Token("1"));
            parser.FailsToParse(Tokenize("("), "");
            parser.FailsToParse(Tokenize("(!"), "!");
            parser.FailsToParse(Tokenize("(1A"), "A");
        }

        [Test]
        public void ApplyingANegativeLookaheadAssertionWithoutConsumingInput()
        {
            var parser = Not(LETTER);

            parser.PartiallyParses(Tokenize("0"), "0").IntoValue(value => value.ShouldBeNull());
            parser.FailsToParse(Tokenize("A"), "A");
        }

        [Test]
        public void ParsingAnOptionalRuleZeroOrOneTimes()
        {
            var parser = Optional(LETTER);

            parser.PartiallyParses(Tokenize("A."), ".").IntoValue(Token("A"));
            parser.PartiallyParses(Tokenize("."), ".").IntoValue(token => token.ShouldBeNull());
        }

        [Test]
        public void ParsingARuleOnlyWhenItsResultWouldPassesAPredicate()
        {
            Predicate<Token> isDollars = x => x.Literal == "$";
            Predicate<Token> isCents = x => x.Literal == "¢";

            Expect(SYMBOL, isDollars).FailsToParse(Tokenize("!"), "!");
            Expect(SYMBOL, isCents).FailsToParse(Tokenize("!"), "!");

            Expect(SYMBOL, isDollars).Parses(Tokenize("$")).IntoValue(Token("$"));
            Expect(SYMBOL, isCents).Parses(Tokenize("¢")).IntoValue(Token("¢"));

            Expect(SYMBOL, isDollars).FailsToParse(Tokenize("¢"), "¢");
            Expect(SYMBOL, isCents).FailsToParse(Tokenize("$"), "$");
        }

        [Test]
        public void ChoosingTheFirstSuccessfulParserFromAPrioritizedList()
        {
            Parser<Token> A = LETTER;
            Parser<Token> AB = from a in LETTER
                               from b in LETTER
                               select new Token(null, a.Position, a.Literal + b.Literal);
            Parser<Token> ABC = from a in LETTER
                                from b in LETTER
                                from c in LETTER
                                select new Token(null, a.Position, a.Literal + b.Literal + c.Literal);

            Parser<Token> parenthesizedA = Between(SYMBOL, A, SYMBOL);
            Parser<Token> parenthesizedAB = Between(SYMBOL, AB, SYMBOL);
            Parser<Token> parenthesizedABC = Between(SYMBOL, ABC, SYMBOL);

            Parser<Token> choice = Choice(
                OnError(parenthesizedA, "parenthesized a"),
                OnError(parenthesizedAB, "parenthesized ab"),
                OnError(parenthesizedABC, "parenthesized abc"));
            
            choice.PartiallyParses(Tokenize("(a)bcd"), "bcd").IntoValue(Token("a")); //First rule wins.
            choice.PartiallyParses(Tokenize("(ab)cd"), "cd").IntoValue(Token("ab")); //Second rule wins.
            choice.PartiallyParses(Tokenize("(abc)d"), "d").IntoValue(Token("abc")); //Third rule wins.

            //When all rules fail, the error returned should correspond with the
            //rule that made it deepest into the input before encountering a failure.
            choice.FailsToParse(Tokenize("(a1"), "1").WithMessage("(1, 3): parenthesized a expected"); //First rule's error wins.
            choice.FailsToParse(Tokenize("(ab1"), "1").WithMessage("(1, 4): parenthesized ab expected"); //Second rule's error wins.
            choice.FailsToParse(Tokenize("(abc1"), "1").WithMessage("(1, 5): parenthesized abc expected"); //Third rule's error wins.

            //When all rules fail, and there is a tie while selecting the rule that 
            //made it deepest into the input, favor the rules in the order they were
            //declared.
            choice.FailsToParse(Tokenize("(1"), "1").WithMessage("(1, 2): parenthesized a expected"); //First rule's error wins.
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
            LETTER.FailsToParse(Tokenize("."), ".").WithMessage("(1, 1): Parse error.");

            OnError(LETTER, "letter").FailsToParse(Tokenize("."), ".").WithMessage("(1, 1): letter expected");
        }

        [Test]
        public void ProvidingTheCurrentPositionWithoutConsumingInput()
        {
            Position.PartiallyParses(Tokenize("A"), "A").IntoValue(position =>
            {
                position.Line.ShouldEqual(1);
                position.Column.ShouldEqual(1);
            });

            var afterLeadingWhiteSpace =
                from _ in WHITESPACE
                from position in Position
                select position;

            afterLeadingWhiteSpace.PartiallyParses(Tokenize("  \r\n   \r\n   !"), "!").IntoValue(position =>
            {
                position.Line.ShouldEqual(3);
                position.Column.ShouldEqual(4);
            });
        }
    }
}