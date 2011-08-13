using Parsley;

namespace Rook.Compiling.Syntax
{
    public static class ParsingAssertions
    {
        public static Reply<T> Parses<T>(this Parser<T> parser, string source)
        {
            return parser.Parses(new RookLexer(source));
        }

        public static Reply<T> FailsToParse<T>(this Parser<T> parser, string source, string expectedUnparsedSource)
        {
            return parser.FailsToParse(new RookLexer(source), expectedUnparsedSource);
        }

        public static void IntoTree<TSyntax>(this Reply<TSyntax> reply, string expectedSyntaxTree) where TSyntax : SyntaxTree
        {
            reply.IntoValue(syntaxTree => syntaxTree.Visit(new Serializer()).ShouldEqual(expectedSyntaxTree));
        }
    }
}