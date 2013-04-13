using System;
using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Syntax;
using Should;

namespace Rook.Compiling.Types
{
    public class NamedTypeTests
    {
        public void HasAName()
        {
            Create("A").Name.ShouldEqual("A");
            Create("B", Create("A")).Name.ShouldEqual("B");
        }

        public void HasZeroOrMoreGenericArguments()
        {
            Create("A").GenericArguments.ShouldBeEmpty();

            Create("B", Create("A")).GenericArguments.ShouldList(Create("A"));

            Create("C", Create("B", Create("A"))).GenericArguments.ShouldList(Create("B", Create("A")));
        }

        public void HasAStringRepresentation()
        {
            Create("A").ToString().ShouldEqual("A");
            Create("A", Create("B")).ToString().ShouldEqual("A<B>");
            Create("A", Create("B", Create("C"), Create("D"))).ToString().ShouldEqual("A<B<C, D>>");
        }

        public void IsGenericWhenGenericArgumentsExist()
        {
            Create("A").IsGeneric.ShouldBeFalse();

            Create("B", Create("A")).IsGeneric.ShouldBeTrue();

            Create("C", Create("B", Create("A"))).IsGeneric.ShouldBeTrue();
        }

        public void HasValueEqualitySemantics()
        {
            var type = Create("B", Create("A"));
            type.ShouldEqual(type);
            type.ShouldEqual(Create("B", Create("A")));
            type.ShouldNotEqual(Create("B"));
            type.ShouldNotEqual(new TypeVariable(0));

            type.GetHashCode().ShouldEqual(Create("B", Create("A")).GetHashCode());
            type.GetHashCode().ShouldNotEqual(Create("B").GetHashCode());
        }

        public void CanBeCreatedFromConvenienceFactories()
        {
            NamedType.Void.ShouldEqual(Create("Rook.Core.Void"));
            NamedType.Integer.ShouldEqual(Create("int"));
            NamedType.Boolean.ShouldEqual(Create("bool"));
            NamedType.String.ShouldEqual(Create("string"));
            NamedType.Enumerable(NamedType.Integer).ShouldEqual(Create("System.Collections.Generic.IEnumerable", Create("int")));
            NamedType.Vector(NamedType.Integer).ShouldEqual(Create("Rook.Core.Collections.Vector", Create("int")));
            NamedType.Nullable(NamedType.Integer).ShouldEqual(Create("Rook.Core.Nullable", Create("int")));
            NamedType.Function(NamedType.Integer).ShouldEqual(Create("System.Func", Create("int")));
            NamedType.Function(new[] { NamedType.Boolean, NamedType.Enumerable(NamedType.Boolean) }, NamedType.Integer)
                .ShouldEqual(Create("System.Func", Create("bool"), Create("System.Collections.Generic.IEnumerable", Create("bool")), Create("int")));
            NamedType.Constructor(Create("A")).ShouldEqual(Create("Rook.Core.Constructor", Create("A")));
        }

        public void CanBeConstructedFromEmptyRookClassDeclarations()
        {
            var @class = "class Foo { }".ParseClass();

            new NamedType(@class).ShouldEqual("Foo", "Foo");
        }

        public void CanBeConstructedFromRookClassDeclarationsIncludingMethods()
        {
            var @class = "class Foo { int Square(int x) {x*x} }".ParseClass();

            var foo = new NamedType(@class, new TypeRegistry());
            foo.ShouldEqual("Foo", "Foo");

            foo.Methods.ShouldList(
                method =>
                {
                    method.Identifier.ShouldEqual("Square");
                    method.Type.ShouldEqual(NamedType.Function(new[] {NamedType.Integer}, NamedType.Integer));
                });
        }

        public void CanBeConstructedFromNongenericClrTypes()
        {
            new NamedType(typeof(int)).ShouldEqual("System.Int32", "int");
        }

        public void CanBeConstructedFromOpenGenericClrTypes()
        {
            using (TypeVariable.TestFactory())
            {
                var openEnumerable = new NamedType(typeof(IEnumerable<>));

                openEnumerable.ShouldEqual(
                    "System.Collections.Generic.IEnumerable",
                    "System.Collections.Generic.IEnumerable<0>",
                    new TypeVariable(0));
            }
        }

        public void UsesFreshTypeVariablesUponEachConstructionFromAnOpenGenericClrType()
        {
            using (TypeVariable.TestFactory())
            {
                var enumerableT = new NamedType(typeof(IEnumerable<>));
                var enumerableS = new NamedType(typeof(IEnumerable<>));

                var T = enumerableT.GenericArguments.Single();
                var S = enumerableS.GenericArguments.Single();

                enumerableT.ShouldNotEqual(enumerableS);
                T.ShouldNotEqual(S);

                T.ShouldEqual(new TypeVariable(0));
                S.ShouldEqual(new TypeVariable(1));
            }
        }

        public void CanBeConstructedFromClosedGenericClrTypes()
        {
            var closedEnumerable = new NamedType(typeof(IEnumerable<int>));

            closedEnumerable.ShouldEqual(
                "System.Collections.Generic.IEnumerable",
                "System.Collections.Generic.IEnumerable<int>",
                NamedType.Integer);
        }

        public void CannotBeConstructedFromGenericParameterTypeObjects()
        {
            var T = typeof(IEnumerable<>).GetGenericArguments().Single();
            Action nameTheUnnamable = () => new NamedType(T);
            nameTheUnnamable.ShouldThrow<ArgumentException>("NamedType cannot be constructed for generic parameter: T");
        }

        public void CanDistinguishGenericTypeDefinitionsFromSpecializations()
        {
            var intType = new NamedType(typeof(int));
            var closedEnumerable = new NamedType(typeof(IEnumerable<int>));
            var openEnumerable = new NamedType(typeof(IEnumerable<>));
            var nonClrType = Create("A", new TypeVariable(0));

            intType.IsGenericTypeDefinition.ShouldBeFalse();
            closedEnumerable.IsGenericTypeDefinition.ShouldBeFalse();
            openEnumerable.IsGenericTypeDefinition.ShouldBeTrue();
            nonClrType.IsGenericTypeDefinition.ShouldBeFalse();
        }

        public void CanBeConstructedFromSpecializingAGenericTypeDefinition()
        {
            using (TypeVariable.TestFactory())
            {
                Action nonGenericOrigin = () => NamedType.Integer.MakeGenericType();
                nonGenericOrigin.ShouldThrow<InvalidOperationException>("int is not a generic type definition, so it cannot be used to make generic types.");

                Action closedGenericOrigin = () => new NamedType(typeof(IEnumerable<int>)).MakeGenericType();
                closedGenericOrigin.ShouldThrow<InvalidOperationException>("System.Collections.Generic.IEnumerable<int> is not a generic type definition, so it cannot be used to make generic types.");

                Action invalidTypeArgumentCount = () => new NamedType(typeof(IEnumerable<>)).MakeGenericType(NamedType.Integer, NamedType.Boolean);
                invalidTypeArgumentCount.ShouldThrow<ArgumentException>("Invalid number of generic type arguments.");

                new NamedType(typeof(IEnumerable<>)).MakeGenericType(NamedType.Integer)
                    .ShouldEqual(new NamedType(typeof(IEnumerable<int>)));

                new NamedType(typeof(IDictionary<,>)).MakeGenericType(NamedType.String, NamedType.Integer)
                    .ShouldEqual(new NamedType(typeof(IDictionary<string, int>)));
            }
        }

        public void CanDetermineWhetherTheTypeContainsASpecificTypeVariable()
        {
            var x = new TypeVariable(12345);

            Create("A").Contains(x).ShouldBeFalse();
            Create("A", x).Contains(x).ShouldBeTrue();
            Create("A", Create("B", x)).Contains(x).ShouldBeTrue();
        }

        public void CanFindAllDistinctOccurrencesOfContainedTypeVariables()
        {
            var x = new TypeVariable(0);
            var y = new TypeVariable(1);
            var z = new TypeVariable(2);

            Create("A").FindTypeVariables().ShouldBeEmpty();
            Create("A", Create("B")).FindTypeVariables().ShouldBeEmpty();
            Create("A", x, y, z).FindTypeVariables().ShouldList(x, y, z);
            Create("A", Create("B", x, y), Create("C", y, z)).FindTypeVariables().ShouldList(x, y, z);
        }

        public void CanPerformTypeVariableSubstitutions()
        {
            var a = new TypeVariable(0);
            var b = new TypeVariable(1);

            var replaceAWithInteger = new Dictionary<TypeVariable, DataType> { { a, NamedType.Integer } };
            var replaceBWithA = new Dictionary<TypeVariable, DataType> { { b, a } };
            var replaceBoth = new Dictionary<TypeVariable, DataType> { { a, NamedType.Integer }, { b, a } };

            var concrete = Create("A", Create("B"));
            concrete.ReplaceTypeVariables(replaceAWithInteger).ShouldEqual(concrete);
            concrete.ReplaceTypeVariables(replaceBWithA).ShouldEqual(concrete);
            concrete.ReplaceTypeVariables(replaceBoth).ShouldEqual(concrete);

            Create("A", a, b, a).ReplaceTypeVariables(replaceAWithInteger).ShouldEqual(Create("A", NamedType.Integer, b, NamedType.Integer));
            Create("B", b, a, b).ReplaceTypeVariables(replaceAWithInteger).ShouldEqual(Create("B", b, NamedType.Integer, b));

            Create("A", a, b, a).ReplaceTypeVariables(replaceBWithA).ShouldEqual(Create("A", a, a, a));
            Create("B", b, a, b).ReplaceTypeVariables(replaceBWithA).ShouldEqual(Create("B", a, a, a));
            
            //Unlike the type unification/normlization substitutions, these substitutions are ignorant of
            //chains like { b -> a, a -> int }.
            Create("A", a, b, a).ReplaceTypeVariables(replaceBoth).ShouldEqual(Create("A", NamedType.Integer, a, NamedType.Integer));
            Create("B", b, a, b).ReplaceTypeVariables(replaceBoth).ShouldEqual(Create("B", a, NamedType.Integer, a));
        }

        public void PerformsTypeVariableSubstitutionsAgainstNonGenericTypesByPerformingNoChanges()
        {
            var @class = "class Foo { int Square(int x) {x*x} }".ParseClass();

            var foo = new NamedType(@class, new TypeRegistry());

            var a = new TypeVariable(0);
            var replaceAWithInteger = new Dictionary<TypeVariable, DataType> { { a, NamedType.Integer } };

            var fooAfterSubstitutions = (NamedType)foo.ReplaceTypeVariables(replaceAWithInteger);

            fooAfterSubstitutions.ShouldBeSameAs(foo);
        }

        private static DataType Create(string name, params DataType[] genericArguments)
        {
            return new NamedType(name, genericArguments);
        }
    }
}