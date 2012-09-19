using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public static class StringExtensions
    {
        public static Class ParseClass(this string source)
        {
            var tokens = Tokenize(source);
            var parser = new RookGrammar().Class;
            return parser.Parse(tokens).Value;
        }

        public static TokenStream Tokenize(this string source)
        {
            return new TokenStream(new RookLexer().Tokenize(source));
        }
    }
}
