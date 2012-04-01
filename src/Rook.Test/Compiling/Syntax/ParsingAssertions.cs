using Parsley;
using Should;

namespace Rook.Compiling.Syntax
{
    public static class ParsingAssertions
    {
        public static Reply<T> Parses<T>(this Parser<T> parser, string source)
        {
            var tokens = new RookLexer().Tokenize(source);
            return parser.Parses(tokens);
        }

        public static Reply<T> FailsToParse<T>(this Parser<T> parser, string source)
        {
            var tokens = new RookLexer().Tokenize(source);
            return parser.FailsToParse(tokens);
        }

        public static void IntoTree<TSyntax>(this Reply<TSyntax> reply, string expectedSyntaxTree) where TSyntax : SyntaxTree
        {
            reply.WithValue(syntaxTree => syntaxTree.Visit(new Serializer()).ShouldEqual(expectedSyntaxTree));
        }
    }
}