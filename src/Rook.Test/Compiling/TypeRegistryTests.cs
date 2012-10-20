using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Should;

namespace Rook.Compiling
{
    [Facts]
    public class TypeRegistryTests
    {
        private readonly TypeRegistry typeRegistry;

        public TypeRegistryTests()
        {
            typeRegistry = new TypeRegistry();
        }

        public void ShouldGetNullForUnregisteredTypes()
        {
            var bogusType = typeRegistry.TypeOf(new TypeName("ThisTypeDoesNotExist"));
            bogusType.ShouldBeNull();
        }

        public void ShouldGetKeywordTypes()
        {
            typeRegistry.TypeOf(TypeName.Integer).ShouldEqual("System.Int32", "int");
            typeRegistry.TypeOf(TypeName.Boolean).ShouldEqual("System.Boolean", "bool");
            typeRegistry.TypeOf(TypeName.String).ShouldEqual("System.String", "string");
            typeRegistry.TypeOf(TypeName.Void).ShouldEqual("Rook.Core.Void", "Rook.Core.Void");
        }

        public void ShouldGetTypesForRegisteredClasses()
        {
            var math = "class Math { int Square(int x) x*x; bool Zero(int x) x==0; }".ParseClass();

            typeRegistry.Add(math);
            typeRegistry.TypeOf(new TypeName("Math")).ShouldEqual(new NamedType(math, typeRegistry));
        }

        public void ShouldGetClosedEnumerableTypesForKnownItemTypes()
        {
            var closedEnumerable = typeRegistry.TypeOf(TypeName.Enumerable(TypeName.Integer));

            closedEnumerable.ShouldEqual("System.Collections.Generic.IEnumerable",
                                         "System.Collections.Generic.IEnumerable<int>",
                                         NamedType.Integer);
        }

        public void ShouldGetClosedVectorTypesForKnownItemTypes()
        {
            var closedVector = typeRegistry.TypeOf(TypeName.Vector(TypeName.Integer));

            closedVector.ShouldEqual("Rook.Core.Collections.Vector",
                                     "Rook.Core.Collections.Vector<int>",
                                     NamedType.Integer);
        }

        public void ShouldGetClosedNullableTypesForKnownItemTypes()
        {
            var closedNullable = typeRegistry.TypeOf(TypeName.Nullable(TypeName.Integer));

            closedNullable.ShouldEqual("Rook.Core.Nullable",
                                       "Rook.Core.Nullable<int>",
                                       NamedType.Integer);
        }

        public void ShouldGetNestedClosedTypesForWellKnownGenericTypes()
        {
            var nestedTypeName =
                TypeName.Vector(
                    TypeName.Enumerable(
                        TypeName.Nullable(TypeName.Integer)));

            var nestedType = typeRegistry.TypeOf(nestedTypeName);

            nestedType.ShouldEqual("Rook.Core.Collections.Vector",
                                   "Rook.Core.Collections.Vector<System.Collections.Generic.IEnumerable<Rook.Core.Nullable<int>>>",
                                   NamedType.Enumerable(NamedType.Nullable(NamedType.Integer)));
        }

        public void ShouldGetNullForWellKnownGenericTypesWithUnregisteredGenericTypeArguments()
        {
            var vectorOfFoo = TypeName.Vector(new TypeName("Foo"));

            typeRegistry.TypeOf(vectorOfFoo).ShouldBeNull();
        }

        public void ShouldGetDeclaredFuncTypeForFunctionDeclaration()
        {
            var function = @"bool FunctionWithIrrelevantBody(int* a, int[] b) false".ParseFunction();

            typeRegistry.DeclaredType(function)
                .ShouldEqual("System.Func",
                             "System.Func<System.Collections.Generic.IEnumerable<int>, Rook.Core.Collections.Vector<int>, bool>",
                             NamedType.Enumerable(NamedType.Integer),
                             NamedType.Vector(NamedType.Integer),
                             NamedType.Boolean);
        }
    }
}