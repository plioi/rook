using Parsley;
using Rook.Compiling.Syntax;

namespace Rook
{
    public static class StringExtensions
    {
        public static Class ParseClass(this string source)
        {
            var tokens = new RookLexer().Tokenize(source);
            var parser = new RookGrammar().Class;
            return parser.Parse(new TokenStream(tokens)).Value;
        }
    }
}