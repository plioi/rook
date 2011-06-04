using NUnit.Framework;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class TypeNameParserSpec
    {
        private static readonly Parser<NamedType> TypeName = Grammar.TypeName;
        private static readonly NamedType Integer = NamedType.Integer;
        private static readonly NamedType Boolean = NamedType.Boolean;
        private static readonly NamedType Void = NamedType.Void;
        private static readonly NamedType Foo = NamedType.Create("Foo");
        
        [Test]
        public void DemandsSimpleNameAtAMinimum()
        {
            TypeName.FailsToParse("", "").WithMessage("(1, 1): type name expected");
            TypeName.FailsToParse("?", "?").WithMessage("(1, 1): type name expected");
            TypeName.FailsToParse("*", "*").WithMessage("(1, 1): type name expected");
            TypeName.FailsToParse("[]", "[]").WithMessage("(1, 1): type name expected");
        }

        [Test]
        public void ParsesSimpleTypeNames()
        {
            TypeName.Parses("int").IntoValue(Integer);
            TypeName.Parses("bool").IntoValue(Boolean);
            TypeName.Parses("void").IntoValue(Void);
            TypeName.Parses("Foo").IntoValue(Foo);
        }

        [Test]
        public void ParsesNullableTypeNames()
        {
            TypeName.Parses("int?").IntoValue(NamedType.Nullable(Integer));
            TypeName.Parses("bool?").IntoValue(NamedType.Nullable(Boolean));
            TypeName.Parses("Foo?").IntoValue(NamedType.Nullable(Foo));
        }

        [Test]
        public void ParsesEnumerableTypeNames()
        {
            TypeName.Parses("int*").IntoValue(NamedType.Enumerable(Integer));
            TypeName.Parses("bool*").IntoValue(NamedType.Enumerable(Boolean));
            TypeName.Parses("Foo**").IntoValue(NamedType.Enumerable(NamedType.Enumerable(Foo)));
        }

        [Test]
        public void ParsesVectorTypeNames()
        {
            TypeName.Parses("int[]").IntoValue(NamedType.Vector(Integer));
            TypeName.Parses("bool[]").IntoValue(NamedType.Vector(Boolean));
            TypeName.Parses("Foo[][]").IntoValue(NamedType.Vector(NamedType.Vector(Foo)));
        }

        [Test]
        public void ParsesTypeNamesWithMixedModifiers()
        {
            TypeName.Parses("int*?").IntoValue(NamedType.Nullable(NamedType.Enumerable(Integer)));
            TypeName.Parses("bool?*").IntoValue(NamedType.Enumerable(NamedType.Nullable(Boolean)));

            TypeName.Parses("int[]?").IntoValue(NamedType.Nullable(NamedType.Vector(Integer)));
            TypeName.Parses("bool?[]").IntoValue(NamedType.Vector(NamedType.Nullable(Boolean)));

            TypeName.Parses("int*[]").IntoValue(NamedType.Vector(NamedType.Enumerable(Integer)));
            TypeName.Parses("bool[]*").IntoValue(NamedType.Enumerable(NamedType.Vector(Boolean)));
        }
    }
}