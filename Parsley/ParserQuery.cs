using System;

namespace Parsley
{
	public static class ParserQuery
	{
		//Unit: The parser which leaves the input alone and simply returns the given value. 
		public static Parser<T> ToParser<T>(this T value)
		{
			return text => new Success<T>(value, text);
		}

		//Bind: Propogate the unpacking of Parsed<T> and the passing along of remaining unparsed text.
		public static Parser<U> SelectMany<T, U>(this Parser<T> parse, Func<T, Parser<U>> constructNextParser)
		{
            return text => parse(text).ParseRest(constructNextParser);
		}

		//Generalized bind overload for performance reasons.
		//This is always defined the same way, in terms of SelectMany<T, U>.
		public static Parser<V> SelectMany<T, U, V>(this Parser<T> parse, Func<T, Parser<U>> k, Func<T, U, V> s)
		{
			return parse.SelectMany(x => k(x).SelectMany(y => s(x, y).ToParser()));
		}

		//Support queries with a single from clause.
		public static Parser<U> Select<T, U>(this Parser<T> parse, Func<T, U> constructResult)
		{
			return parse.SelectMany(t => constructResult(t).ToParser());
		}
	}
}