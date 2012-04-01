using Parsley;
using Should;

namespace Rook.Compiling.Syntax
{
    public static class ParsingAssertions
    {
        public static Reply<T> Parses<T>(this Parser<T> parser, string source)
        {
            var tokens = new RookLexer().Tokenize(new Text(source));
            return parser.Parses(new TokenStream(tokens));
        }

        public static Reply<T> FailsToParse<T>(this Parser<T> parser, string source)
        {
            var tokens = new RookLexer().Tokenize(new Text(source));
            return parser.FailsToParse(new TokenStream(tokens));
        }

        public static void IntoTree<TSyntax>(this Reply<TSyntax> reply, string expectedSyntaxTree) where TSyntax : SyntaxTree
        {
            reply.IntoValue(syntaxTree => syntaxTree.Visit(new Serializer()).ShouldEqual(expectedSyntaxTree));
        }
    }
}