using System.Collections.Generic;
using Parsley;
using Rook.Core;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    [Facts]
    public class TypeNameParserTests
    {
        private static readonly TypeName Integer = new TypeName("System.Int32");
        private static readonly TypeName Boolean = new TypeName("System.Boolean");
        private static readonly TypeName String = new TypeName("System.String");
        private static readonly TypeName Void = new TypeName("Rook.Core.Void");
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
            Parses("int").WithValue(Integer);
            Parses("bool").WithValue(Boolean);
            Parses("string").WithValue(String);
            Parses("void").WithValue(Void);
            Parses("Foo").WithValue(Foo);
        }

        public void ParsesNullableTypeNames()
        {
            Parses("int?").WithValue(Nullable(Integer));
            Parses("bool?").WithValue(Nullable(Boolean));
            Parses("Foo?").WithValue(Nullable(Foo));
        }

        public void ParsesEnumerableTypeNames()
        {
            Parses("int*").WithValue(Enumerable(Integer));
            Parses("bool*").WithValue(Enumerable(Boolean));
            Parses("Foo**").WithValue(Enumerable(Enumerable(Foo)));
        }

        public void ParsesVectorTypeNames()
        {
            Parses("int[]").WithValue(Vector(Integer));
            Parses("bool[]").WithValue(Vector(Boolean));
            Parses("Foo[][]").WithValue(Vector(Vector(Foo)));
        }

        public void ParsesTypeNamesWithMixedModifiers()
        {
            Parses("int*?").WithValue(Nullable(Enumerable(Integer)));
            Parses("bool?*").WithValue(Enumerable(Nullable(Boolean)));

            Parses("int[]?").WithValue(Nullable(Vector(Integer)));
            Parses("bool?[]").WithValue(Vector(Nullable(Boolean)));

            Parses("int*[]").WithValue(Vector(Enumerable(Integer)));
            Parses("bool[]*").WithValue(Enumerable(Vector(Boolean)));
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