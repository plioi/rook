using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    [Facts]
    public class TypeNameParserTests
    {
        private static readonly NamedType Integer = NamedType.Integer;
        private static readonly NamedType Boolean = NamedType.Boolean;
        private static readonly NamedType String = NamedType.String;
        private static readonly NamedType Void = NamedType.Void;
        private static readonly NamedType Enumerable = NamedType.Enumerable;
        private static readonly NamedType Vector = NamedType.Vector;
        private static readonly NamedType Nullable = NamedType.Nullable;
        private static readonly NamedType Constructor = NamedType.Constructor;
        private static readonly NamedType Foo = new NamedType("Foo");
        
        public void DemandsSimpleNameAtAMinimum()
        {
            FailsToParse("").AtEndOfInput().WithMessage("(1, 1): type name expected");
            FailsToParse("?").LeavingUnparsedTokens("?").WithMessage("(1, 1): type name expected");
            FailsToParse("*").LeavingUnparsedTokens("*").WithMessage("(1, 1): type name expected");
            FailsToParse("[]").LeavingUnparsedTokens("[]").WithMessage("(1, 1): type name expected");
        }

        public void ParsesSimpleTypeNames()
        {
            Parses("int").WithValue(Integer);
            Parses("bool").WithValue(Boolean);
            Parses("string").WithValue(String);
            Parses("void").WithValue(Void);
            Parses("Foo").WithValue(Foo);
        }

        public void ParsesNullableTypeNames()
        {
            Parses("int?").WithValue(Nullable.MakeGenericType(Integer));
            Parses("bool?").WithValue(Nullable.MakeGenericType(Boolean));
            Parses("Foo?").WithValue(Nullable.MakeGenericType(Foo));
        }

        public void ParsesEnumerableTypeNames()
        {
            Parses("int*").WithValue(Enumerable.MakeGenericType(Integer));
            Parses("bool*").WithValue(Enumerable.MakeGenericType(Boolean));
            Parses("Foo**").WithValue(Enumerable.MakeGenericType(Enumerable.MakeGenericType(Foo)));
        }

        public void ParsesVectorTypeNames()
        {
            Parses("int[]").WithValue(Vector.MakeGenericType(Integer));
            Parses("bool[]").WithValue(Vector.MakeGenericType(Boolean));
            Parses("Foo[][]").WithValue(Vector.MakeGenericType(Vector.MakeGenericType(Foo)));
        }

        public void ParsesTypeNamesWithMixedModifiers()
        {
            Parses("int*?").WithValue(Nullable.MakeGenericType(Enumerable.MakeGenericType(Integer)));
            Parses("bool?*").WithValue(Enumerable.MakeGenericType(Nullable.MakeGenericType(Boolean)));

            Parses("int[]?").WithValue(Nullable.MakeGenericType(Vector.MakeGenericType(Integer)));
            Parses("bool?[]").WithValue(Vector.MakeGenericType(Nullable.MakeGenericType(Boolean)));

            Parses("int*[]").WithValue(Vector.MakeGenericType(Enumerable.MakeGenericType(Integer)));
            Parses("bool[]*").WithValue(Enumerable.MakeGenericType(Vector.MakeGenericType(Boolean)));
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