using NUnit.Framework;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class TypeNameParserSpec
    {
        private static readonly NamedType Integer = NamedType.Integer;
        private static readonly NamedType Boolean = NamedType.Boolean;
        private static readonly NamedType Void = NamedType.Void;
        private static readonly NamedType Foo = new NamedType("Foo");
        
        [Test]
        public void DemandsSimpleNameAtAMinimum()
        {
            FailsToParse("", "").WithMessage("(1, 1): type name expected");
            FailsToParse("?", "?").WithMessage("(1, 1): type name expected");
            FailsToParse("*", "*").WithMessage("(1, 1): type name expected");
            FailsToParse("[]", "[]").WithMessage("(1, 1): type name expected");
        }

        [Test]
        public void ParsesSimpleTypeNames()
        {
            Parses("int").IntoValue(Integer);
            Parses("bool").IntoValue(Boolean);
            Parses("void").IntoValue(Void);
            Parses("Foo").IntoValue(Foo);
        }

        [Test]
        public void ParsesNullableTypeNames()
        {
            Parses("int?").IntoValue(NamedType.Nullable(Integer));
            Parses("bool?").IntoValue(NamedType.Nullable(Boolean));
            Parses("Foo?").IntoValue(NamedType.Nullable(Foo));
        }

        [Test]
        public void ParsesEnumerableTypeNames()
        {
            Parses("int*").IntoValue(NamedType.Enumerable(Integer));
            Parses("bool*").IntoValue(NamedType.Enumerable(Boolean));
            Parses("Foo**").IntoValue(NamedType.Enumerable(NamedType.Enumerable(Foo)));
        }

        [Test]
        public void ParsesVectorTypeNames()
        {
            Parses("int[]").IntoValue(NamedType.Vector(Integer));
            Parses("bool[]").IntoValue(NamedType.Vector(Boolean));
            Parses("Foo[][]").IntoValue(NamedType.Vector(NamedType.Vector(Foo)));
        }

        [Test]
        public void ParsesTypeNamesWithMixedModifiers()
        {
            Parses("int*?").IntoValue(NamedType.Nullable(NamedType.Enumerable(Integer)));
            Parses("bool?*").IntoValue(NamedType.Enumerable(NamedType.Nullable(Boolean)));

            Parses("int[]?").IntoValue(NamedType.Nullable(NamedType.Vector(Integer)));
            Parses("bool?[]").IntoValue(NamedType.Vector(NamedType.Nullable(Boolean)));

            Parses("int*[]").IntoValue(NamedType.Vector(NamedType.Enumerable(Integer)));
            Parses("bool[]*").IntoValue(NamedType.Enumerable(NamedType.Vector(Boolean)));
        }

        private static Reply<NamedType> FailsToParse(string source, string expectedUnparsedSource)
        {
            return Grammar.TypeName.FailsToParse(source, expectedUnparsedSource);
        }

        private static Reply<NamedType> Parses(string source)
        {
            return Grammar.TypeName.Parses(source);
        }
    }
}