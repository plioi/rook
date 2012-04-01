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
            FailsToParse("").AtEndOfInput().WithMessage("(1, 1): type name expected");
            FailsToParse("?").LeavingUnparsedTokens("?").WithMessage("(1, 1): type name expected");
            FailsToParse("*").LeavingUnparsedTokens("*").WithMessage("(1, 1): type name expected");
            FailsToParse("[]").LeavingUnparsedTokens("[]").WithMessage("(1, 1): type name expected");
        }

        [Fact]
        public void ParsesSimpleTypeNames()
        {
            Parses("int").WithValue(Integer);
            Parses("bool").WithValue(Boolean);
            Parses("string").WithValue(String);
            Parses("void").WithValue(Void);
            Parses("Foo").WithValue(Foo);
        }

        [Fact]
        public void ParsesNullableTypeNames()
        {
            Parses("int?").WithValue(NamedType.Nullable(Integer));
            Parses("bool?").WithValue(NamedType.Nullable(Boolean));
            Parses("Foo?").WithValue(NamedType.Nullable(Foo));
        }

        [Fact]
        public void ParsesEnumerableTypeNames()
        {
            Parses("int*").WithValue(NamedType.Enumerable(Integer));
            Parses("bool*").WithValue(NamedType.Enumerable(Boolean));
            Parses("Foo**").WithValue(NamedType.Enumerable(NamedType.Enumerable(Foo)));
        }

        [Fact]
        public void ParsesVectorTypeNames()
        {
            Parses("int[]").WithValue(NamedType.Vector(Integer));
            Parses("bool[]").WithValue(NamedType.Vector(Boolean));
            Parses("Foo[][]").WithValue(NamedType.Vector(NamedType.Vector(Foo)));
        }

        [Fact]
        public void ParsesTypeNamesWithMixedModifiers()
        {
            Parses("int*?").WithValue(NamedType.Nullable(NamedType.Enumerable(Integer)));
            Parses("bool?*").WithValue(NamedType.Enumerable(NamedType.Nullable(Boolean)));

            Parses("int[]?").WithValue(NamedType.Nullable(NamedType.Vector(Integer)));
            Parses("bool?[]").WithValue(NamedType.Vector(NamedType.Nullable(Boolean)));

            Parses("int*[]").WithValue(NamedType.Vector(NamedType.Enumerable(Integer)));
            Parses("bool[]*").WithValue(NamedType.Enumerable(NamedType.Vector(Boolean)));
        }

        private static Reply<NamedType> FailsToParse(string source)
        {
            return new RookGrammar().TypeName.FailsToParse(source);
        }

        private static Reply<NamedType> Parses(string source)
        {
            return new RookGrammar().TypeName.Parses(source);
        }
    }
}