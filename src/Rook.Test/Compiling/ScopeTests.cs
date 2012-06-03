using System;
using Parsley;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Rook.Core.Collections;
using Should;
using Xunit;

namespace Rook.Compiling
{
    public class ScopeTests
    {
        private static readonly NamedType Integer = NamedType.Integer;
        private static readonly NamedType Boolean = NamedType.Boolean;

        private readonly Scope root, ab, bc;

        public ScopeTests()
        {
            root = new Scope();

            ab = new Scope(root);
            ab["a"] = Integer;
            ab["b"] = Integer;

            bc = new Scope(ab);
            bc["b"] = Boolean;
            bc["c"] = Boolean;
        }

        [Fact]
        public void StoresLocals()
        {
            AssertType(Integer, ab, "a");
            AssertType(Integer, ab, "b");
        }

        [Fact]
        public void DefersToSurroundingScopeAfterSearchingLocals()
        {
            AssertType(Integer, bc, "a");
            AssertType(Boolean, bc, "b");
            AssertType(Boolean, bc, "c");
        }

        [Fact]
        public void AllowsOverwritingInsideLocals()
        {
            ab["b"] = Boolean;
            bc["b"] = Integer;

            AssertType(Boolean, ab, "b");
            AssertType(Integer, bc, "b");
        }

        [Fact]
        public void ProvidesContainmentPredicate()
        {
            ab.Contains("a").ShouldBeTrue();
            ab.Contains("b").ShouldBeTrue();
            ab.Contains("c").ShouldBeFalse();
            ab.Contains("z").ShouldBeFalse();

            bc.Contains("a").ShouldBeTrue();
            bc.Contains("b").ShouldBeTrue();
            bc.Contains("c").ShouldBeTrue();
            bc.Contains("z").ShouldBeFalse();
        }

        [Fact]
        public void ProvidesFactoryMethodToCreateChildScopeWithBuiltinSignatures()
        {
            var scope = Scope.CreateScopeWithBuiltins(root);

            AssertType("System.Func<int, int, bool>", scope, "<");
            AssertType("System.Func<int, int, bool>", scope, "<=");
            AssertType("System.Func<int, int, bool>", scope, ">");
            AssertType("System.Func<int, int, bool>", scope, ">=");
            AssertType("System.Func<int, int, bool>", scope, "==");
            AssertType("System.Func<int, int, bool>", scope, "!=");

            AssertType("System.Func<int, int, int>", scope, "+");
            AssertType("System.Func<int, int, int>", scope, "-");
            AssertType("System.Func<int, int, int>", scope, "*");
            AssertType("System.Func<int, int, int>", scope, "/");

            AssertType("System.Func<bool, bool, bool>", scope, "||");
            AssertType("System.Func<bool, bool, bool>", scope, "&&");
            AssertType("System.Func<bool, bool>", scope, "!");

            AssertType("System.Func<Rook.Core.Nullable<0>, 0, 0>", scope, "??");
            AssertType("System.Func<0, Rook.Core.Void>", scope, "Print");
            AssertType("System.Func<0, Rook.Core.Nullable<0>>", scope, "Nullable");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, 0>", scope, "First");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, int, System.Collections.Generic.IEnumerable<0>>", scope, "Take");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, int, System.Collections.Generic.IEnumerable<0>>", scope, "Skip");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, bool>", scope, "Any");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, int>", scope, "Count");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, System.Func<0, 1>, System.Collections.Generic.IEnumerable<1>>", scope, "Select");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, System.Func<0, bool>, System.Collections.Generic.IEnumerable<0>>", scope, "Where");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, System.Collections.Generic.IEnumerable<0>>", scope, "Each");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, int, 0>", scope, "Index");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, int, int, Rook.Core.Collections.Vector<0>>", scope, "Slice");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, 0, Rook.Core.Collections.Vector<0>>", scope, "Append");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, int, 0, Rook.Core.Collections.Vector<0>>", scope, "With");
        }

        [Fact]
        public void IncludesOptionalSetOfTypeMemberBindingsInTheRootScope()
        {
            var foo = new NamedType("Foo");
            var math = new NamedType("Math");

            var fooBinding = new StubTypeMemberBinding(foo,
                                                       new StubBinding("I", NamedType.Function(Integer)),
                                                       new StubBinding("B", NamedType.Function(Boolean)));


            var mathBinding = new StubTypeMemberBinding(math,
                                                        new StubBinding("Square", NamedType.Function(new[] {Integer}, Integer)),
                                                        new StubBinding("Even", NamedType.Function(new[] {Integer}, Boolean)));

            var rootWithTypes = new Scope(new[] {fooBinding, mathBinding});
            var childScope = new Scope(rootWithTypes);

            AssertMemberType(NamedType.Function(Integer), rootWithTypes, foo, "I");
            AssertMemberType(NamedType.Function(Boolean), rootWithTypes, foo, "B");
            AssertMemberType(NamedType.Function(new[] { Integer }, Integer), rootWithTypes, math, "Square");
            AssertMemberType(NamedType.Function(new[] { Integer }, Boolean), rootWithTypes, math, "Even");

            AssertMemberType(NamedType.Function(Integer), childScope, foo, "I");
            AssertMemberType(NamedType.Function(Boolean), childScope, foo, "B");
            AssertMemberType(NamedType.Function(new[] { Integer }, Integer), childScope, math, "Square");
            AssertMemberType(NamedType.Function(new[] { Integer }, Boolean), childScope, math, "Even");

            Scope expectedFailure;
            rootWithTypes.TryGetMemberScope(new NamedType("UnknownType"), out expectedFailure).ShouldBeFalse();
            expectedFailure.ShouldBeNull();
        }

        [Fact]
        public void ProvidesStreamOfUniqueTypeVariables()
        {
            root.CreateTypeVariable().ShouldEqual(new TypeVariable(0));
            root.CreateTypeVariable().ShouldEqual(new TypeVariable(1));
            ab.CreateTypeVariable().ShouldEqual(new TypeVariable(2));
            bc.CreateTypeVariable().ShouldEqual(new TypeVariable(3));
            ab.CreateTypeVariable().ShouldEqual(new TypeVariable(4));
            bc.CreateTypeVariable().ShouldEqual(new TypeVariable(5));
        }

        [Fact]
        public void CanBePopulatedWithAUniqueBinding()
        {
            root.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")).ShouldBeTrue();
            root.TryIncludeUniqueBinding(new Parameter(null, Boolean, "b")).ShouldBeTrue();
            AssertType(Integer, root, "a");
            AssertType(Boolean, root, "b");
        }

        [Fact]
        public void DemandsUniqueBindingsWhenIncludingUniqueBindings()
        {
            root.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")).ShouldBeTrue();
            root.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")).ShouldBeFalse();
            root.TryIncludeUniqueBinding(new Parameter(null, Boolean, "a")).ShouldBeFalse();
            AssertType(Integer, root, "a");
        }

        [Fact]
        public void CanDetermineWhetherAGivenTypeVariableIsGenericWhenPreparedWithAKnownListOfNonGenericTypeVariables()
        {
            TypeVariable var0 = root.CreateTypeVariable();
            TypeVariable var1 = root.CreateTypeVariable();
            TypeVariable var2 = root.CreateTypeVariable();
            TypeVariable var3 = root.CreateTypeVariable();

            root.TreatAsNonGeneric(new[] {var0});
            ab.TreatAsNonGeneric(new[] {var1, var2});
            bc.TreatAsNonGeneric(new[] {var3});

            root.IsGeneric(var0).ShouldBeFalse();
            root.IsGeneric(var1).ShouldBeTrue();
            root.IsGeneric(var2).ShouldBeTrue();
            root.IsGeneric(var3).ShouldBeTrue();

            ab.IsGeneric(var0).ShouldBeFalse();
            ab.IsGeneric(var1).ShouldBeFalse();
            ab.IsGeneric(var2).ShouldBeFalse();
            ab.IsGeneric(var3).ShouldBeTrue();

            bc.IsGeneric(var0).ShouldBeFalse();
            bc.IsGeneric(var1).ShouldBeFalse();
            bc.IsGeneric(var2).ShouldBeFalse();
            bc.IsGeneric(var3).ShouldBeFalse();
        }

        private static void AssertType(DataType expectedType, Scope scope, string key)
        {
            DataType value;

            if (scope.TryGet(key, out value))
                value.ShouldEqual(expectedType);
            else
                throw new Exception("Failed to look up the type of '" + key + "' in the Scope");
        }

        private static void AssertMemberType(DataType expectedType, Scope scope, DataType typeKey, string memberKey)
        {
            Scope typeMemberScope;

            if (scope.TryGetMemberScope(typeKey, out typeMemberScope))
                AssertType(expectedType, typeMemberScope, memberKey);
            else
                throw new Exception("Failed to look up the type of '" + typeKey + "+" + memberKey + "' in the Scope");
        }

        private static void AssertType(string expectedType, Scope scope, string key)
        {
            DataType value;

            if (scope.TryGet(key, out value))
                expectedType.ShouldEqual(value.ToString());
            else
                throw new Exception("Failed to look up the type of '" + key + "' in the Scope");
        }

        private class StubTypeMemberBinding : TypeMemberBinding
        {
            public StubTypeMemberBinding(DataType type, params Binding[] members)
            {
                Type = type;
                Members = members.ToVector();
            }

            public DataType Type { get; private set; }
            public Vector<Binding> Members { get; private set; }
        }

        private class StubBinding : Binding
        {
            public StubBinding(string identifier, DataType type)
            {
                Position = null;
                Identifier = identifier;
                Type = type;
            }

            public Position Position{ get; private set; }
            public string Identifier{ get; private set; }
            public DataType Type { get; private set; }
        }
    }
}