using System;
using Parsley;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Rook.Core.Collections;
using Should;
using Xunit;

namespace Rook.Compiling
{
    public class EnvironmentTests
    {
        private static readonly NamedType Integer = NamedType.Integer;
        private static readonly NamedType Boolean = NamedType.Boolean;

        private readonly Environment root, ab, bc;

        public EnvironmentTests()
        {
            root = new Environment();

            ab = new Environment(root);
            ab["a"] = Integer;
            ab["b"] = Integer;

            bc = new Environment(ab);
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
        public void ProvidesFactoryMethodToCreateChildEnvironmentWithBuiltinSignatures()
        {
            var environment = Environment.CreateEnvironmentWithBuiltins(root);

            AssertType("System.Func<int, int, bool>", environment, "<");
            AssertType("System.Func<int, int, bool>", environment, "<=");
            AssertType("System.Func<int, int, bool>", environment, ">");
            AssertType("System.Func<int, int, bool>", environment, ">=");
            AssertType("System.Func<int, int, bool>", environment, "==");
            AssertType("System.Func<int, int, bool>", environment, "!=");

            AssertType("System.Func<int, int, int>", environment, "+");
            AssertType("System.Func<int, int, int>", environment, "-");
            AssertType("System.Func<int, int, int>", environment, "*");
            AssertType("System.Func<int, int, int>", environment, "/");

            AssertType("System.Func<bool, bool, bool>", environment, "||");
            AssertType("System.Func<bool, bool, bool>", environment, "&&");
            AssertType("System.Func<bool, bool>", environment, "!");

            AssertType("System.Func<Rook.Core.Nullable<0>, 0, 0>", environment, "??");
            AssertType("System.Func<1, Rook.Core.Void>", environment, "Print");
            AssertType("System.Func<2, Rook.Core.Nullable<2>>", environment, "Nullable");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<3>, 3>", environment, "First");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<4>, int, System.Collections.Generic.IEnumerable<4>>", environment, "Take");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<5>, int, System.Collections.Generic.IEnumerable<5>>", environment, "Skip");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<6>, bool>", environment, "Any");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<7>, int>", environment, "Count");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<8>, System.Func<8, 9>, System.Collections.Generic.IEnumerable<9>>", environment, "Select");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<10>, System.Func<10, bool>, System.Collections.Generic.IEnumerable<10>>", environment, "Where");
            AssertType("System.Func<Rook.Core.Collections.Vector<11>, System.Collections.Generic.IEnumerable<11>>", environment, "Each");
            AssertType("System.Func<Rook.Core.Collections.Vector<12>, int, 12>", environment, "Index");
            AssertType("System.Func<Rook.Core.Collections.Vector<13>, int, int, Rook.Core.Collections.Vector<13>>", environment, "Slice");
            AssertType("System.Func<Rook.Core.Collections.Vector<14>, 14, Rook.Core.Collections.Vector<14>>", environment, "Append");
            AssertType("System.Func<Rook.Core.Collections.Vector<15>, int, 15, Rook.Core.Collections.Vector<15>>", environment, "With");
        }

        [Fact]
        public void IncludesOptionalSetOfTypeMemberBindingsInTheRootEnvironment()
        {
            var foo = new NamedType("Foo");
            var math = new NamedType("Math");

            var fooBinding = new StubTypeMemberBinding(foo,
                                                       new StubBinding("I", NamedType.Function(Integer)),
                                                       new StubBinding("B", NamedType.Function(Boolean)));


            var mathBinding = new StubTypeMemberBinding(math,
                                                        new StubBinding("Square", NamedType.Function(new[] {Integer}, Integer)),
                                                        new StubBinding("Even", NamedType.Function(new[] {Integer}, Boolean)));

            var rootWithTypes = new Environment(new[] {fooBinding, mathBinding});
            var childEnvironment = new Environment(rootWithTypes);

            AssertMemberType(NamedType.Function(Integer), rootWithTypes, foo, "I");
            AssertMemberType(NamedType.Function(Boolean), rootWithTypes, foo, "B");
            AssertMemberType(NamedType.Function(new[] { Integer }, Integer), rootWithTypes, math, "Square");
            AssertMemberType(NamedType.Function(new[] { Integer }, Boolean), rootWithTypes, math, "Even");

            AssertMemberType(NamedType.Function(Integer), childEnvironment, foo, "I");
            AssertMemberType(NamedType.Function(Boolean), childEnvironment, foo, "B");
            AssertMemberType(NamedType.Function(new[] { Integer }, Integer), childEnvironment, math, "Square");
            AssertMemberType(NamedType.Function(new[] { Integer }, Boolean), childEnvironment, math, "Even");

            Environment expectedFailure;
            rootWithTypes.TryGetMemberEnvironment(new NamedType("UnknownType"), out expectedFailure).ShouldBeFalse();
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
        public void ProvidesTypeNormalizerSharedWithAllLocalEnvironments()
        {
            ab.TypeNormalizer.ShouldBeSameAs(root.TypeNormalizer);
            bc.TypeNormalizer.ShouldBeSameAs(ab.TypeNormalizer);
            new Environment().TypeNormalizer.ShouldNotBeSameAs(root.TypeNormalizer);
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

        private static void AssertType(DataType expectedType, Environment environment, string key)
        {
            DataType value;

            if (environment.TryGet(key, out value))
                value.ShouldEqual(expectedType);
            else
                throw new Exception("Failed to look up the type of '" + key + "' in the environment");
        }

        private static void AssertMemberType(DataType expectedType, Environment environment, DataType typeKey, string memberKey)
        {
            Environment typeMemberEnvironment;

            if (environment.TryGetMemberEnvironment(typeKey, out typeMemberEnvironment))
                AssertType(expectedType, typeMemberEnvironment, memberKey);
            else
                throw new Exception("Failed to look up the type of '" + typeKey + "+" + memberKey + "' in the environment");
        }

        private static void AssertType(string expectedType, Environment environment, string key)
        {
            DataType value;

            if (environment.TryGet(key, out value))
                expectedType.ShouldEqual(value.ToString());
            else
                throw new Exception("Failed to look up the type of '" + key + "' in the environment");
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