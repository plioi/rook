using System;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public static class StringExtensions
    {
        public static Class ParseClass(this string source)
        {
            return source.Parse(g => g.Class);
        }

        public static Function ParseFunction(this string source)
        {
            return source.Parse(g => g.Function);
        }

        private static T Parse<T>(this string source, Func<RookGrammar, Parser<T>> getParser)
        {
            var tokens = Tokenize(source);
            var parser = getParser(new RookGrammar());
            return parser.Parse(tokens).Value;
        }

        public static TokenStream Tokenize(this string source)
        {
            return new TokenStream(new RookLexer().Tokenize(source));
        }
    }
}
