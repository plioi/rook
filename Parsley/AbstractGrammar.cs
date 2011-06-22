using System;
using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public abstract class AbstractGrammar
    {
        public static Parser<T> Fail<T>()
        {
            return tokens => new Error<T>(tokens, ErrorMessage.Unknown());
        }

        public static Parser<Token> EndOfInput
        {
            get { return Kind(Lexer.EndOfInput); }
        }

        public static Parser<Token> Kind(TokenKind kind)
        {
            return tokens =>
            {
                if (tokens.CurrentToken.Kind == kind)
                    return new Parsed<Token>(tokens.CurrentToken, tokens.Advance());

                return new Error<Token>(tokens, ErrorMessage.Expected(kind.Name));
            };
        }

        public static Parser<Token> String(string expectation)
        {
            return tokens =>
            {
                if (tokens.CurrentToken.Literal == expectation)
                    return new Parsed<Token>(tokens.CurrentToken, tokens.Advance());

                return new Error<Token>(tokens, ErrorMessage.Expected(expectation));
            };
        }

        public static Parser<IEnumerable<T>> ZeroOrMore<T>(Parser<T> item)
        {
            return tokens =>
            {
                var tokensBeforeFirstFailure = tokens;
                var reply = item(tokens);
                var list = new List<T>();

                while (reply.Success)
                {
                    list.Add(reply.Value);
                    tokensBeforeFirstFailure = reply.UnparsedTokens;
                    reply = item(reply.UnparsedTokens);
                }

                return new Parsed<IEnumerable<T>>(list, tokensBeforeFirstFailure);
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
                   from pairs in ZeroOrMore(from s in separator
                                            from i in item
                                            select Tuple.Create(s, i))
                   select pairs.Aggregate(first, associatePair);
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

        public static Parser<T> Optional<T>(Parser<T> parse) where T : class
        {
            var nothing = default(T).SucceedWithThisValue();
            return Choice(parse, nothing);
        }

        /// <summary>
        /// The parser Attempt(p) behaves like parser p, except that it pretends
        /// that it hasn't consumed any input when an error occurs. This combinator
        /// is used whenever arbitrary look ahead is needed.
        /// </summary>
        public static Parser<T> Attempt<T>(Parser<T> parse)
        {
            return tokens =>
            {
                var start = tokens.Position;
                var reply = parse(tokens);
                var newPosition = reply.UnparsedTokens.Position;

                if (reply.Success || start == newPosition)
                    return reply;

                //Backtrack to original position.

                //TODO: Backtracking (by returning 'tokens' instead of reply.UnparsedTokens below) is correct.
                //      Ideally, though, the error message reported would be more accurate by indicating
                //      the true position of error.  FParsec uses 'nested errors' to support this concept:
                //
                //      There are two positions we want to return:
                //          a) The original position, meaning "nothing was consumed, so keep running from that position again".
                //          b) The position that the error message text really belongs to.
                //
                //      These 2 positions are the usually the same, except when a backtracking-combinator like Attempt actually backtracks.

                var backtrackingError = reply.ErrorMessages; //TODO: var backtrackingError = NestedError(reply.UnparsedTokens{.Position?}, reply.ErrorMessages)
                return new Error<T>(tokens, backtrackingError);
            };
        }

        /// <summary>
        /// Choice() always fails without consuming input.
        /// 
        /// Choice(p) is equivalent to p.
        /// 
        /// For 2 or more inputs, parsers are applied from left
        /// to right.  If a parser succeeds, its result is returned.
        /// If a parser fails without consuming input, the next parser
        /// is attempted.  If a parser fails after consuming input,
        /// subsequent parsers will not be attempted.  As long as
        /// parsers conume no input, their error messages are merged.
        ///
        /// Choice is 'predictive' since p[n+1] is only tried when
        /// p[n] didn't consume any input (i.e. the look-ahead is 1).
        /// This non-backtracking behaviour allows for both an efficient
        /// implementation of the parser combinators and the generation
        /// of good error messages.
        /// </summary>
        public static Parser<T> Choice<T>(params Parser<T>[] parsers)
        {
            if (parsers.Length == 0)
                return Fail<T>();

            return tokens =>
            {
                var start = tokens.Position;
                var reply = parsers[0](tokens);
                var newPosition = reply.UnparsedTokens.Position;

                var errors = ErrorMessageList.Empty;
                var i = 1;
                while (!reply.Success && (start == newPosition) && i < parsers.Length)
                {
                    errors = errors.Merge(reply.ErrorMessages);
                    reply = parsers[i](tokens);
                    newPosition = reply.UnparsedTokens.Position;
                    i++;
                }
                if (start == newPosition)
                {
                    errors = errors.Merge(reply.ErrorMessages);
                    if (reply.Success)
                        reply = new Parsed<T>(reply.Value, reply.UnparsedTokens, errors);
                    else
                        reply = new Error<T>(reply.UnparsedTokens, errors);
                }

                return reply;
            };
        }

        public static Parser<T> OnError<T>(Parser<T> parse, string expectation)
        {
            return tokens =>
            {
                Reply<T> reply = parse(tokens);

                if (reply.Success)
                    return reply;

                return new Error<T>(reply.UnparsedTokens, ErrorMessage.Expected(expectation));
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