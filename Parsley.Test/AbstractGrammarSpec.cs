using System;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class AlternationSpec : AbstractGrammar
    {
        private static Lexer Tokenize(string source)
        {
            return new CharLexer(source);
        }

        private Parser<Token> A, B, C;

        [SetUp]
        public void Setup()
        {
            A = String("A");
            B = String("B");
            C = String("C");
        }

        [Test]
        public void ChoosingBetweenZeroAlternativesAlwaysFails()
        {
            Choice<string>().FailsToParse(Tokenize("ABC"), "ABC");
        }

        [Test]
        public void ChoosingBetweenOneAlternativeParserIsEquivalentToThatParser()
        {
            Choice(A).Parses(Tokenize("A")).IntoToken("A");
            Choice(A).PartiallyParses(Tokenize("AB"), "B").IntoToken("A");
            Choice(A).FailsToParse(Tokenize("B"), "B").WithMessage("(1, 1): A expected");
        }

        [Test]
        public void FirstParserCanSucceedWithoutExecutingOtherAlternatives()
        {
            Choice(A, NeverExecuted).Parses(Tokenize("A")).IntoToken("A");
        }

        [Test]
        public void SubsequentParserCanSucceedWhenPreviousParsersFailWithoutConsumingInput()
        {
            Choice(B, A).Parses(Tokenize("A")).IntoToken("A");
            Choice(C, B, A).Parses(Tokenize("A")).IntoToken("A");
        }

        [Test]
        public void SubsequentParserWillNotBeAttemptedWhenPreviousParserFailsAfterConsumingInput()
        {
            //As soon as something consumes input, it's failure and message win.

            var AB = from a in A
                     from b in B
                     select new Token(null, a.Position, a.Literal + b.Literal);

            Choice(AB, NeverExecuted).FailsToParse(Tokenize("A"), "").WithMessage("(1, 2): B expected");
            Choice(C, AB, NeverExecuted).FailsToParse(Tokenize("A"), "").WithMessage("(1, 2): B expected");
        }

        [Test]
        public void MergesErrorMessagesWhenParsersFailWithoutConsumingInput()
        {
            Choice(A, B).FailsToParse(Tokenize(""), "").WithMessage("(1, 1): A or B expected");
            Choice(A, B, C).FailsToParse(Tokenize(""), "").WithMessage("(1, 1): A, B or C expected");
        }

        [Test]
        public void MergesPotentialErrorMessagesWhenParserSucceedsWithoutConsumingInput()
        {
            //Choice really shouldn't be used with parsers that can succeed without
            //consuming input.  These tests simply describe the behavior under that
            //unusual situation.

            Parser<Token> succeedWithoutConsuming = tokens => new Parsed<Token>(null, tokens);

            var reply = Choice(A, succeedWithoutConsuming).Parses(Tokenize(""));
            reply.ErrorMessages.ToString().ShouldEqual("A expected");

            reply = Choice(A, B, succeedWithoutConsuming).Parses(Tokenize(""));
            reply.ErrorMessages.ToString().ShouldEqual("A or B expected");

            reply = Choice(A, succeedWithoutConsuming, B).Parses(Tokenize(""));
            reply.ErrorMessages.ToString().ShouldEqual("A expected");
        }

        private static readonly Parser<Token> NeverExecuted = tokens =>
        {
            throw new Exception("Parser 'NeverExecuted' should not have been executed.");
        };
    }

    [TestFixture]
    public class AbstractGrammarSpec : AbstractGrammar
    {
        private static Lexer Tokenize(string source)
        {
            return new SampleLexer(source);
        }

        private static Parser<Token> DIGIT { get { return Kind(SampleLexer.Digit); } }
        private static Parser<Token> LETTER { get { return Kind(SampleLexer.Letter); } }
        private static Parser<Token> COMMA { get { return Kind(SampleLexer.Comma); } }
        private static Parser<Token> WHITESPACE { get { return Kind(SampleLexer.WhiteSpace); } }
        private static Parser<Token> SYMBOL { get { return Kind(SampleLexer.Symbol); } }

        private class SampleLexer : Lexer
        {
            public static readonly TokenKind Digit = new TokenKind("Digit", @"[0-9]");
            public static readonly TokenKind Letter = new TokenKind("Letter", @"[a-zA-Z]");
            public static readonly TokenKind Comma = new TokenKind("Comma", @"\,");
            public static readonly TokenKind WhiteSpace = new TokenKind("WhiteSpace", @"\s+");
            public static readonly TokenKind Symbol = new TokenKind("Symbol", @".");

            public SampleLexer(string source)
                : base(new Text(source), Digit, Letter, Comma, WhiteSpace, Symbol) { }
        }

        [Test]
        public void CanFailWithoutConsumingInput()
        {
            Fail<string>().FailsToParse(Tokenize("ABC"), "ABC");
        }

        [Test]
        public void CanDetectTheEndOfInputWithoutAdvancing()
        {
            EndOfInput.Parses(Tokenize("")).IntoToken("");
            EndOfInput.FailsToParse(Tokenize("!"), "!");
        }

        [Test]
        public void CanDemandThatAGivenKindOfTokenAppearsNext()
        {
            Kind(SampleLexer.Letter).Parses(Tokenize("A")).IntoToken("A");
            Kind(SampleLexer.Letter).FailsToParse(Tokenize("0"), "0");

            Kind(SampleLexer.Digit).FailsToParse(Tokenize("A"), "A");
            Kind(SampleLexer.Digit).Parses(Tokenize("0")).IntoToken("0");
        }

        [Test]
        public void CanDemandThatAGivenTokenStringAppearsNext()
        {
            String("A").Parses(Tokenize("A")).IntoToken("A");
            String("\t ").PartiallyParses(Tokenize("\t !"), "!").IntoToken("\t ");
            String("A").FailsToParse(Tokenize("B"), "B").WithMessage("(1, 1): A expected");
        }

        [Test]
        public void ApplyingARuleFollowedByARequiredButDiscardedTerminatorRule()
        {
            Parser<Token> parser = LETTER.TerminatedBy(SYMBOL);

            parser.PartiallyParses(Tokenize("A~Unparsed"), "Unparsed").IntoToken("A");
            parser.FailsToParse(Tokenize(""), "");
            parser.FailsToParse(Tokenize("~"), "~");
            parser.FailsToParse(Tokenize("A0"), "0");
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimes()
        {
            var parser = ZeroOrMore(DIGIT);

            parser.Parses(Tokenize("")).IntoTokens();
            parser.PartiallyParses(Tokenize("!"), "!").IntoTokens();

            parser.PartiallyParses(Tokenize("0!"), "!").IntoTokens("0");
            parser.PartiallyParses(Tokenize("01!"), "!").IntoTokens("0", "1");
            parser.PartiallyParses(Tokenize("012!"), "!").IntoTokens("0", "1", "2");

            parser.Parses(Tokenize("0")).IntoTokens("0");
            parser.Parses(Tokenize("01")).IntoTokens("0", "1");
            parser.Parses(Tokenize("012")).IntoTokens("0", "1", "2");
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimes()
        {
            var parser = OneOrMore(DIGIT);

            parser.FailsToParse(Tokenize(""), "");
            parser.FailsToParse(Tokenize("!"), "!");

            parser.PartiallyParses(Tokenize("0!"), "!").IntoTokens("0");
            parser.PartiallyParses(Tokenize("01!"), "!").IntoTokens("0", "1");
            parser.PartiallyParses(Tokenize("012!"), "!").IntoTokens("0", "1", "2");

            parser.Parses(Tokenize("0")).IntoTokens("0");
            parser.Parses(Tokenize("01")).IntoTokens("0", "1");
            parser.Parses(Tokenize("012")).IntoTokens("0", "1", "2");
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimesInterspersedByASeparatorRule()
        {
            var parser = ZeroOrMore(DIGIT, COMMA);

            parser.Parses(Tokenize("")).IntoTokens();
            parser.PartiallyParses(Tokenize("!"), "!").IntoTokens();

            parser.PartiallyParses(Tokenize("0!"), "!").IntoTokens("0");
            parser.PartiallyParses(Tokenize("0,1!"), "!").IntoTokens("0", "1");
            parser.PartiallyParses(Tokenize("0,1,2!"), "!").IntoTokens("0", "1", "2");
            parser.PartiallyParses(Tokenize("0,1,2,"), ",").IntoTokens("0", "1", "2");

            parser.Parses(Tokenize("0")).IntoTokens("0");
            parser.Parses(Tokenize("0,1")).IntoTokens("0", "1");
            parser.Parses(Tokenize("0,1,2")).IntoTokens("0", "1", "2");
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimesFollowedByARequiredTerminatorRule()
        {
            var parser = ZeroOrMoreTerminated(DIGIT, SYMBOL);

            parser.FailsToParse(Tokenize(""), "");
            parser.FailsToParse(Tokenize("MissingTerminator"), "MissingTerminator");
            parser.FailsToParse(Tokenize("0MissingTerminator"), "MissingTerminator");
            parser.FailsToParse(Tokenize("01MissingTerminator"), "MissingTerminator");

            parser.Parses(Tokenize("~")).IntoTokens();
            parser.PartiallyParses(Tokenize("~!"), "!").IntoTokens();
            parser.Parses(Tokenize("0~")).IntoTokens("0");
            parser.Parses(Tokenize("01~")).IntoTokens("0", "1");
            parser.PartiallyParses(Tokenize("012~!"), "!").IntoTokens("0", "1", "2");
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
        {
            var parser = OneOrMore(DIGIT, COMMA);

            parser.FailsToParse(Tokenize(""), "");
            parser.FailsToParse(Tokenize("!"), "!");

            parser.PartiallyParses(Tokenize("0!"), "!").IntoTokens("0");
            parser.PartiallyParses(Tokenize("0,1!"), "!").IntoTokens("0", "1");
            parser.PartiallyParses(Tokenize("0,1,2!"), "!").IntoTokens("0", "1", "2");

            parser.Parses(Tokenize("0")).IntoTokens("0");
            parser.Parses(Tokenize("0,1")).IntoTokens("0", "1");
            parser.Parses(Tokenize("0,1,2")).IntoTokens("0", "1", "2");
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimesInterspersedByALeftAssociativeSeparatorRule()
        {
            var parser =
                LeftAssociative(DIGIT, SYMBOL, (left, symbolAndRight) =>
                    new Token(null, symbolAndRight.Item1.Position, System.String.Format("({0} {1} {2})", symbolAndRight.Item1.Literal, left.Literal, symbolAndRight.Item2.Literal)));

            parser.FailsToParse(Tokenize("!"), "!");
            parser.Parses(Tokenize("0")).IntoToken("0");
            parser.Parses(Tokenize("0*1")).IntoToken("(* 0 1)");
            parser.Parses(Tokenize("0*1/2")).IntoToken("(/ (* 0 1) 2)");
        }

        [Test]
        public void ApplyingARuleBetweenTwoOtherRules()
        {
            var parser = Between(SYMBOL, DIGIT, SYMBOL);

            parser.PartiallyParses(Tokenize("(1)Unparsed"), "Unparsed").IntoToken("1");
            parser.FailsToParse(Tokenize("("), "");
            parser.FailsToParse(Tokenize("(!"), "!");
            parser.FailsToParse(Tokenize("(1A"), "A");
        }

        [Test]
        public void ParsingAnOptionalRuleZeroOrOneTimes()
        {
            var A = String("A");
            var B = String("B");

            var AB = from a in A
                     from b in B
                     select new Token(null, a.Position, a.Literal + b.Literal);

            Optional(AB).PartiallyParses(Tokenize("AB."), ".").IntoToken("AB");
            Optional(AB).PartiallyParses(Tokenize("."), ".").IntoValue(token => token.ShouldBeNull());
            Optional(AB).FailsToParse(Tokenize("AC."), "C.").WithMessage("(1, 2): B expected");
        }

        [Test]
        public void GreedilyChoosingTheFirstSuccessfulParserFromAPrioritizedList()
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

            Parser<Token> choice = GreedyChoice(
                OnError(parenthesizedA, "parenthesized a"),
                OnError(parenthesizedAB, "parenthesized ab"),
                OnError(parenthesizedABC, "parenthesized abc"));
            
            choice.PartiallyParses(Tokenize("(a)bcd"), "bcd").IntoToken("a"); //First rule wins.
            choice.PartiallyParses(Tokenize("(ab)cd"), "cd").IntoToken("ab"); //Second rule wins.
            choice.PartiallyParses(Tokenize("(abc)d"), "d").IntoToken("abc"); //Third rule wins.

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
        public void DemandsAtLeastOneChoiceWhenBuildingAGreedyChoiceParser()
        {
            GreedyChoice(new Parser<string>[] {});
        }

        [Test]
        public void ImprovingDefaultErrorMessagesWithAKnownExpectation()
        {
            LETTER.FailsToParse(Tokenize("."), ".").WithMessage("(1, 1): Parse error.");

            OnError(LETTER, "letter").FailsToParse(Tokenize("."), ".").WithMessage("(1, 1): letter expected");
        }
    }
}