﻿using System;
using System.Collections.Generic;
using System.Linq;
using Should;
using Xunit;

namespace Rook.Compiling.Types
{
    public class NamedTypeTests
    {
        [Fact]
        public void HasAName()
        {
            Create("A").Name.ShouldEqual("A");
            Create("B", Create("A")).Name.ShouldEqual("B");
        }

        [Fact]
        public void HasZeroOrMoreGenericArguments()
        {
            Create("A").GenericArguments.ShouldBeEmpty();

            Create("B", Create("A")).GenericArguments.ShouldList(Create("A"));

            Create("C", Create("B", Create("A"))).GenericArguments.ShouldList(Create("B", Create("A")));
        }

        [Fact]
        public void HasAStringRepresentation()
        {
            Create("A").ToString().ShouldEqual("A");
            Create("A", Create("B")).ToString().ShouldEqual("A<B>");
            Create("A", Create("B", Create("C"), Create("D"))).ToString().ShouldEqual("A<B<C, D>>");
        }

        [Fact]
        public void IsGenericWhenGenericArgumentsExist()
        {
            Create("A").IsGeneric.ShouldBeFalse();

            Create("B", Create("A")).IsGeneric.ShouldBeTrue();

            Create("C", Create("B", Create("A"))).IsGeneric.ShouldBeTrue();
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public void CanBeConstructedFromNongenericClrTypes()
        {
            var intType = new NamedType(typeof(int));
            intType.Name.ShouldEqual("System.Int32");
            intType.GenericArguments.ShouldBeEmpty();
            intType.ToString().ShouldEqual("int");
        }

        [Fact]
        public void CanBeConstructedFromOpenGenericClrTypes()
        {
            using (TypeVariable.TestFactory())
            {
                var openEnumerable = new NamedType(typeof(IEnumerable<>));
                openEnumerable.Name.ShouldEqual("System.Collections.Generic.IEnumerable");
                openEnumerable.GenericArguments.Single().ShouldEqual(new TypeVariable(0));
                openEnumerable.ToString().ShouldEqual("System.Collections.Generic.IEnumerable<0>");
            }
        }

        [Fact]
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

        [Fact]
        public void CanBeConstructedFromClosedGenericClrTypes()
        {
            var closedEnumerable = new NamedType(typeof(IEnumerable<int>));
            closedEnumerable.Name.ShouldEqual("System.Collections.Generic.IEnumerable");
            closedEnumerable.GenericArguments.Single().ShouldEqual(new NamedType(typeof(int)));
            closedEnumerable.ToString().ShouldEqual("System.Collections.Generic.IEnumerable<int>");
        }

        [Fact]
        public void CannotBeConstructedFromGenericParameterTypeObjects()
        {
            var T = typeof(IEnumerable<>).GetGenericArguments().Single();
            Action nameTheUnnamable = () => new NamedType(T);
            nameTheUnnamable.ShouldThrow<ArgumentException>("NamedType cannot be constructed for generic parameter: T");
        }

        [Fact]
        public void CanDetermineWhetherTheTypeContainsASpecificTypeVariable()
        {
            var x = new TypeVariable(12345);

            Create("A").Contains(x).ShouldBeFalse();
            Create("A", x).Contains(x).ShouldBeTrue();
            Create("A", Create("B", x)).Contains(x).ShouldBeTrue();
        }

        [Fact]
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

        [Fact]
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

        private static DataType Create(string name, params DataType[] genericArguments)
        {
            return new NamedType(name, genericArguments);
        }
    }
}