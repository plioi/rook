using Parsley;

namespace Rook.Compiling.Syntax
{
    public static class ParsingAssertions
    {
        public static Parsed<T> Parses<T>(this Parser<T> parse, string source)
        {
            return parse.Parses(new RookLexer(source));
        }

        public static Parsed<T> FailsToParse<T>(this Parser<T> parse, string source, string expectedUnparsedSource)
        {
            return parse.FailsToParse(new RookLexer(source), expectedUnparsedSource);
        }

        public static void IntoTree<TSyntax>(this Parsed<TSyntax> result, string expectedSyntaxTree) where TSyntax : SyntaxTree
        {
            result.IntoValue(syntaxTree => syntaxTree.Visit(new Serializer()).ShouldEqual(expectedSyntaxTree));
        }
    }
}