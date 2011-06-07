using System;
using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public abstract class AbstractGrammar
    {
        public static Parser<Position> Position
        {
            get { return tokens => new Success<Position>(tokens.Position, tokens); }
        }

        public static Parser<string> EndOfInput
        {
            get
            {
                return tokens =>
                {
                    if (tokens.EndOfInput)
                        return new Success<string>("", tokens);

                    return new Error<string>(tokens);
                };
            }
        }

        public static Parser<Token> Kind(TokenKind kind)
        {
            return tokens =>
            {
                if (tokens.CurrentToken.Kind == kind)
                    return new Success<Token>(tokens.CurrentToken, tokens.Advance());

                return new Error<Token>(tokens);
            };
        }

        public static Parser<IEnumerable<T>> ZeroOrMore<T>(Parser<T> item)
        {
            return tokens =>
            {
                var tokensBeforeFirstFailure = tokens;
                var parsed = item(tokens);
                var list = new List<T>();

                while (!parsed.IsError)
                {
                    list.Add(parsed.Value);
                    tokensBeforeFirstFailure = parsed.UnparsedTokens;
                    parsed = item(parsed.UnparsedTokens);
                }

                return new Success<IEnumerable<T>>(list, tokensBeforeFirstFailure);
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
            return tokens =>
            {
                if (parseAhead(tokens).IsError)
                    return new Success<T>(default(T), tokens);

                return new Error<T>(tokens);
            };
        }

        public static Parser<T> Optional<T>(Parser<T> parse) where T : class
        {
            var nothing = default(T).SucceedWithThisValue();
            return Choice(parse, nothing);
        }

        public static Parser<T> Expect<T>(Parser<T> parse, Predicate<T> expectation)
        {
            return tokens =>
            {
                Parsed<T> result = parse(tokens);

                if (result.IsError)
                   return result;

                if (expectation(result.Value))
                   return result;

                return new Error<T>(tokens);
            };
        }

        public static Parser<T> Choice<T>(params Parser<T>[] parsers)
        {           
            if (parsers.Length == 0)
                throw new ArgumentException("Missing choice.");

            return tokens =>
            {
                Parsed<T> deepestParse = null;

                foreach (Parser<T> parse in parsers)
                {
                    Parsed<T> parsed = parse(tokens);

                    if (deepestParse == null)
                        deepestParse = parsed;

                    if (!parsed.IsError)
                        return parsed;

                    Position newParsePosition = parsed.UnparsedTokens.Position;
                    Position deepestParsePosition = deepestParse.UnparsedTokens.Position;
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
            return tokens =>
            {
                Parsed<T> parsed = parse(tokens);

                if (parsed.IsError)
                    return new Error<T>(parsed.UnparsedTokens, expectation);

                return parsed;
            };
        }

        private static IEnumerable<T> List<T>(T first, IEnumerable<T> rest)
        {
            yield return first;

            foreach (T item in rest)
                yield return item;
        }

        protected static Parser<IEnumerable<T>> Zero<T>()
        {
            return Enumerable.Empty<T>().SucceedWithThisValue();
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