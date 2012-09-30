using Parsley;

namespace Rook.Compiling.Syntax
{
    [Facts]
    public class TypeNameParserTests
    {
        private static readonly TypeName Foo = new TypeName("Foo");
        
        public void DemandsSimpleNameAtAMinimum()
        {
            FailsToParse("").AtEndOfInput().WithMessage("(1, 1): type name expected");
            FailsToParse("?").LeavingUnparsedTokens("?").WithMessage("(1, 1): type name expected");
            FailsToParse("*").LeavingUnparsedTokens("*").WithMessage("(1, 1): type name expected");
            FailsToParse("[]").LeavingUnparsedTokens("[]").WithMessage("(1, 1): type name expected");
        }

        public void ParsesSimpleTypeNames()
        {
            Parses("int").WithValue(TypeName.Integer);
            Parses("bool").WithValue(TypeName.Boolean);
            Parses("string").WithValue(TypeName.String);
            Parses("void").WithValue(TypeName.Void);
            Parses("Foo").WithValue(Foo);
        }

        public void ParsesNullableTypeNames()
        {
            Parses("int?").WithValue(Nullable(TypeName.Integer));
            Parses("bool?").WithValue(Nullable(TypeName.Boolean));
            Parses("Foo?").WithValue(Nullable(Foo));
        }

        public void ParsesEnumerableTypeNames()
        {
            Parses("int*").WithValue(Enumerable(TypeName.Integer));
            Parses("bool*").WithValue(Enumerable(TypeName.Boolean));
            Parses("Foo**").WithValue(Enumerable(Enumerable(Foo)));
        }

        public void ParsesVectorTypeNames()
        {
            Parses("int[]").WithValue(Vector(TypeName.Integer));
            Parses("bool[]").WithValue(Vector(TypeName.Boolean));
            Parses("Foo[][]").WithValue(Vector(Vector(Foo)));
        }

        public void ParsesTypeNamesWithMixedModifiers()
        {
            Parses("int*?").WithValue(Nullable(Enumerable(TypeName.Integer)));
            Parses("bool?*").WithValue(Enumerable(Nullable(TypeName.Boolean)));

            Parses("int[]?").WithValue(Nullable(Vector(TypeName.Integer)));
            Parses("bool?[]").WithValue(Vector(Nullable(TypeName.Boolean)));

            Parses("int*[]").WithValue(Vector(Enumerable(TypeName.Integer)));
            Parses("bool[]*").WithValue(Enumerable(Vector(TypeName.Boolean)));
        }

        private static Reply<TypeName> FailsToParse(string source)
        {
            return new RookGrammar().TypeName.FailsToParse(source);
        }

        private static Reply<TypeName> Parses(string source)
        {
            return new RookGrammar().TypeName.Parses(source);
        }

        private static TypeName Enumerable(TypeName itemType)
        {
            return new TypeName("System.Collections.Generic.IEnumerable", itemType);
        }

        private static TypeName Vector(TypeName itemType)
        {
            return new TypeName("Rook.Core.Collections.Vector", itemType);
        }

        private static TypeName Nullable(TypeName itemType)
        {
            return new TypeName("Rook.Core.Nullable", itemType);
        }
    }
}