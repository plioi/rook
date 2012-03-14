using Parsley;
using Rook.Compiling.Types;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class TypeNameParserTests
    {
        private static readonly NamedType Integer = NamedType.Integer;
        private static readonly NamedType Boolean = NamedType.Boolean;
        private static readonly NamedType String = NamedType.String;
        private static readonly NamedType Void = NamedType.Void;
        private static readonly NamedType Foo = new NamedType("Foo");
        
        [Fact]
        public void DemandsSimpleNameAtAMinimum()
        {
            FailsToParse("", "").WithMessage("(1, 1): type name expected");
            FailsToParse("?", "?").WithMessage("(1, 1): type name expected");
            FailsToParse("*", "*").WithMessage("(1, 1): type name expected");
            FailsToParse("[]", "[]").WithMessage("(1, 1): type name expected");
        }

        [Fact]
        public void ParsesSimpleTypeNames()
        {
            Parses("int").IntoValue(Integer);
            Parses("bool").IntoValue(Boolean);
            Parses("string").IntoValue(String);
            Parses("void").IntoValue(Void);
            Parses("Foo").IntoValue(Foo);
        }

        [Fact]
        public void ParsesNullableTypeNames()
        {
            Parses("int?").IntoValue(NamedType.Nullable(Integer));
            Parses("bool?").IntoValue(NamedType.Nullable(Boolean));
            Parses("Foo?").IntoValue(NamedType.Nullable(Foo));
        }

        [Fact]
        public void ParsesEnumerableTypeNames()
        {
            Parses("int*").IntoValue(NamedType.Enumerable(Integer));
            Parses("bool*").IntoValue(NamedType.Enumerable(Boolean));
            Parses("Foo**").IntoValue(NamedType.Enumerable(NamedType.Enumerable(Foo)));
        }

        [Fact]
        public void ParsesVectorTypeNames()
        {
            Parses("int[]").IntoValue(NamedType.Vector(Integer));
            Parses("bool[]").IntoValue(NamedType.Vector(Boolean));
            Parses("Foo[][]").IntoValue(NamedType.Vector(NamedType.Vector(Foo)));
        }

        [Fact]
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
            return new RookGrammar().TypeName.FailsToParse(source, expectedUnparsedSource);
        }

        private static Reply<NamedType> Parses(string source)
        {
            return new RookGrammar().TypeName.Parses(source);
        }
    }
}