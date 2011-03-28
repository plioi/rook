using NUnit.Framework;
using Parsley;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class TypeNameParserSpec
    {
        [Test]
        public void DemandsSimpleNameAtAMinimum()
        {
            AssertError("", "", "(1, 1): type name expected");
            AssertError("?", "?", "(1, 1): type name expected");
            AssertError("*", "*", "(1, 1): type name expected");
            AssertError("[]", "[]", "(1, 1): type name expected");
        }

        [Test]
        public void ParsesSimpleTypeNames()
        {
            AssertType("int", "int");
            AssertType("bool", "bool");
            AssertType("Rook.Core.Void", "void");
            AssertType("Foo", "Foo");
        }

        [Test]
        public void ParsesNullableTypeNames()
        {
            AssertType("Rook.Core.Nullable<int>", "int?");
            AssertType("Rook.Core.Nullable<bool>", "bool?");
            AssertType("Rook.Core.Nullable<Foo>", "Foo?");
        }

        [Test]
        public void ParsesEnumerableTypeNames()
        {
            AssertType("System.Collections.Generic.IEnumerable<int>", "int*");
            AssertType("System.Collections.Generic.IEnumerable<bool>", "bool*");
            AssertType("System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<Foo>>", "Foo**");
        }

        [Test]
        public void ParsesVectorTypeNames()
        {
            AssertType("Rook.Core.Collections.Vector<int>", "int[]");
            AssertType("Rook.Core.Collections.Vector<bool>", "bool[]");
            AssertType("Rook.Core.Collections.Vector<Rook.Core.Collections.Vector<Foo>>", "Foo[][]");
        }

        [Test]
        public void ParsesTypeNamesWithMixedModifiers()
        {
            AssertType("Rook.Core.Nullable<System.Collections.Generic.IEnumerable<int>>", "int*?");
            AssertType("System.Collections.Generic.IEnumerable<Rook.Core.Nullable<bool>>", "bool?*");

            AssertType("Rook.Core.Nullable<Rook.Core.Collections.Vector<int>>", "int[]?");
            AssertType("Rook.Core.Collections.Vector<Rook.Core.Nullable<bool>>", "bool?[]");

            AssertType("Rook.Core.Collections.Vector<System.Collections.Generic.IEnumerable<int>>", "int*[]");
            AssertType("System.Collections.Generic.IEnumerable<Rook.Core.Collections.Vector<bool>>", "bool[]*");
        }

        private static void AssertError(string source, string expectedUnparsedSource, string expectedMessage)
        {
            Grammar.TypeName.AssertError(source, expectedUnparsedSource, expectedMessage);
        }

        private static void AssertType(string expectedSyntaxTree, string source)
        {
            const string expectedUnparsedSource = "";
            Grammar.TypeName.AssertParse(source, expectedSyntaxTree, expectedUnparsedSource);
        }
    }
}