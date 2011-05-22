using System;
using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public abstract class AbstractGrammar
    {
        public static Predicate<char> Digit = Char.IsDigit;
        public static Predicate<char> Letter = Char.IsLetter;
        public static Predicate<char> Lower = Char.IsLower;
        public static Predicate<char> Upper = Char.IsUpper;
        public static Predicate<char> Alphanumeric = Char.IsLetterOrDigit;
        public static Predicate<char> WhiteSpace = Char.IsWhiteSpace;

        public static Parser<Position> Position
        {
            get { return text => new Success<Position>(text.Position, text); }
        }

        public static Parser<string> ZeroOrMore(Predicate<char> match)
        {
            return text => Success(text, text.Count(match));
        }

        public static Parser<string> OneOrMore(Predicate<char> match)
        {
            return text =>
            {
                int sizeOfMatch = text.Count(match);

                if (sizeOfMatch > 0)
                    return Success(text, sizeOfMatch);

                return new Error<string>(text);
            };
        }

        public static Parser<string> String(params string[] possibleMatches)
        {
            return text =>
            {
                foreach (string expected in possibleMatches)
                    if (text.Peek(expected.Length) == expected)
                        return Success(text, expected.Length);

                return new Error<string>(text);
            };
        }

        public static Parser<string> EndOfInput
        {
            get
            {
                return text =>
                {
                    if (text.EndOfInput)
                        return Success(text, 0);

                    return new Error<string>(text);
                };
            }
        }

        public static Parser<IEnumerable<T>> ZeroOrMore<T>(Parser<T> item)
        {
            return text =>
            {
                var textBeforeFirstFailure = text;
                var parsed = item(text);
                var list = new List<T>();

                while (!parsed.IsError)
                {
                    list.Add(parsed.Value);
                    textBeforeFirstFailure = parsed.UnparsedText;
                    parsed = item(parsed.UnparsedText);
                }

                return new Success<IEnumerable<T>>(list, textBeforeFirstFailure);
            };
        }

        public static Parser<IEnumerable<T>> OneOrMore<T>(Parser<T> item)
        {
            return from first in item
                   from rest in ZeroOrMore(item)
                   select List(first, rest);
        }

        public static Parser<IEnumerable<T>> ZeroOrMore<T, S>(Parser<T> item, Parser<S> separator)
        {
            return Choice(OneOrMore(item, separator), Zero<T>());
        }

        public static Parser<IEnumerable<T>> ZeroOrMoreTerminated<T, X>(Parser<T> item, Parser<X> terminator)
        {
            return Choice(from end in terminator
                          from zero in Zero<T>()
                          select zero,

                          from first in item
                          from rest in ZeroOrMoreTerminated(item, terminator)
                          select List(first, rest));
        }

        public static Parser<IEnumerable<T>> OneOrMore<T, S>(Parser<T> item, Parser<S> separator)
        {
            return from first in item
                   from rest in ZeroOrMore(from sep in separator
                                           from next in item
                                           select next)
                   select List(first, rest);
        }

        public static Parser<TAccumulated> LeftAssociative<TAccumulated, TSeparator>(Parser<TAccumulated> item, Parser<TSeparator> separator, Func<TAccumulated, Tuple<TSeparator, TAccumulated>, TAccumulated> associatePair)
        {
            return from first in item
                   from pairs in ZeroOrMore(Pair(separator, item))
                   select pairs.Aggregate(first, associatePair);
        }

        public static Parser<Tuple<TLeft, TRight>> Pair<TLeft, TRight>(Parser<TLeft> left, Parser<TRight> right)
        {
            return from L in left
                   from R in right
                   select Tuple.Create(L, R);
        }

        public static Parser<TGoal> Between<TLeft, TGoal, TRight>(Parser<TLeft> left, 
                                                                  Parser<TGoal> goal, 
                                                                  Parser<TRight> right)
        {
            return from L in left
                   from G in goal
                   from R in right
                   select G;
        }

        public static Parser<T> Not<T>(Parser<T> parseAhead)
        {
            return text =>
            {
                if (parseAhead(text).IsError)
                    return new Success<T>(default(T), text);

                return new Error<T>(text);
            };
        }

        public static Parser<T> Optional<T>(Parser<T> parse) where T : class
        {
            var nothing = default(T).ToParser();
            return Choice(parse, nothing);
        }

        public static Parser<T> Expect<T>(Parser<T> parse, Predicate<T> expectation)
        {
            return text =>
            {
                Parsed<T> result = parse(text);

                if (result.IsError)
                   return result;

                if (expectation(result.Value))
                   return result;

                return new Error<T>(text);
            };
        }

        public static Parser<T> Choice<T>(params Parser<T>[] parsers)
        {           
            if (parsers.Length == 0)
                throw new ArgumentException("Missing choice.");

            return text =>
            {
                Parsed<T> deepestParse = null;

                foreach (Parser<T> parse in parsers)
                {
                    Parsed<T> parsed = parse(text);

                    if (deepestParse == null)
                        deepestParse = parsed;

                    if (!parsed.IsError)
                        return parsed;

                    Position newParsePosition = parsed.UnparsedText.Position;
                    Position deepestParsePosition = deepestParse.UnparsedText.Position;
                    bool newParseIsDeeper = newParsePosition.Line > deepestParsePosition.Line ||
                                            (newParsePosition.Line == deepestParsePosition.Line &&
                                             newParsePosition.Column > deepestParsePosition.Column);
                    if (newParseIsDeeper)
                        deepestParse = parsed;
                }

                return deepestParse;
            };
        }

        public static Parser<T> OnError<T>(Parser<T> parse, string expectation)
        {
            return text =>
            {
                Parsed<T> parsed = parse(text);

                if (parsed.IsError)
                    return new Error<T>(parsed.UnparsedText, expectation);

                return parsed;
            };
        }

        private static Parsed<string> Success(Text text, int characters)
        {
            return new Success<string>(text.Peek(characters), text.Advance(characters));
        }

        private static IEnumerable<T> List<T>(T first, IEnumerable<T> rest)
        {
            yield return first;

            foreach (T item in rest)
                yield return item;
        }

        protected static Parser<IEnumerable<T>> Zero<T>()
        {
            return Enumerable.Empty<T>().ToParser();
        }
    }

    public static class AbstractGrammarExtensions
    {
        public static Parser<T> TerminatedBy<T, S>(this Parser<T> goal, Parser<S> terminator)
        {
            return from G in goal
                   from _ in terminator
                   select G;
        }
    }
}