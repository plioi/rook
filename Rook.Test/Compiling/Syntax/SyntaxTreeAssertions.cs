using Parsley;

namespace Rook.Compiling.Syntax
{
    public static class SyntaxTreeAssertions
    {
        public static void IntoTree<TSyntax>(this Parsed<TSyntax> result, string expectedSyntaxTree) where TSyntax : SyntaxTree
        {
            result.IntoValue(syntaxTree => syntaxTree.Visit(new Serializer()).ShouldEqual(expectedSyntaxTree));
        }
    }
}