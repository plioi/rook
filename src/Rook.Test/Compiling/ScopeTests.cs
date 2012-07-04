using System;
using System.Linq;
using Parsley;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
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
            var typeChecker = new TypeChecker();
            root = Scope.CreateRoot(typeChecker, Enumerable.Empty<Class>());

            ab = root.CreateLocalScope();
            ab["a"] = Integer;
            ab["b"] = Integer;

            bc = ab.CreateLocalScope();
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
        public void ProvidesBuiltinSignaturesInTheRootScope()
        {
            AssertType("System.Func<int, int, bool>", root, "<");
            AssertType("System.Func<int, int, bool>", root, "<=");
            AssertType("System.Func<int, int, bool>", root, ">");
            AssertType("System.Func<int, int, bool>", root, ">=");
            AssertType("System.Func<int, int, bool>", root, "==");
            AssertType("System.Func<int, int, bool>", root, "!=");

            AssertType("System.Func<int, int, int>", root, "+");
            AssertType("System.Func<int, int, int>", root, "-");
            AssertType("System.Func<int, int, int>", root, "*");
            AssertType("System.Func<int, int, int>", root, "/");

            AssertType("System.Func<bool, bool, bool>", root, "||");
            AssertType("System.Func<bool, bool, bool>", root, "&&");
            AssertType("System.Func<bool, bool>", root, "!");

            AssertType("System.Func<Rook.Core.Nullable<0>, 0, 0>", root, "??");
            AssertType("System.Func<0, Rook.Core.Void>", root, "Print");
            AssertType("System.Func<0, Rook.Core.Nullable<0>>", root, "Nullable");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, 0>", root, "First");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, int, System.Collections.Generic.IEnumerable<0>>", root, "Take");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, int, System.Collections.Generic.IEnumerable<0>>", root, "Skip");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, bool>", root, "Any");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, int>", root, "Count");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, System.Func<0, 1>, System.Collections.Generic.IEnumerable<1>>", root, "Select");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, System.Func<0, bool>, System.Collections.Generic.IEnumerable<0>>", root, "Where");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, System.Collections.Generic.IEnumerable<0>>", root, "Each");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, int, 0>", root, "Index");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, int, int, Rook.Core.Collections.Vector<0>>", root, "Slice");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, 0, Rook.Core.Collections.Vector<0>>", root, "Append");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, int, 0, Rook.Core.Collections.Vector<0>>", root, "With");
        }

        [Fact]
        public void IncludesOptionalSetOfTypeMemberBindingsInTheRootScope()
        {
            var foo = new NamedType("Foo");
            var math = new NamedType("Math");

            var fooBinding = ParseClass("class Foo { int I() 0; bool B() true; }");
            var mathBinding = ParseClass("class Math { int Square(int x) x*x; bool Zero(int x) x==0; }");

            var typeChecker = new TypeChecker();
            var rootScope = Scope.CreateRoot(typeChecker, new[] {fooBinding, mathBinding});
            var childScope = rootScope.CreateLocalScope();

            AssertMemberType(NamedType.Function(Integer), rootScope, foo, "I");
            AssertMemberType(NamedType.Function(Boolean), rootScope, foo, "B");
            AssertMemberType(NamedType.Function(new[] { Integer }, Integer), rootScope, math, "Square");
            AssertMemberType(NamedType.Function(new[] { Integer }, Boolean), rootScope, math, "Zero");

            AssertMemberType(NamedType.Function(Integer), childScope, foo, "I");
            AssertMemberType(NamedType.Function(Boolean), childScope, foo, "B");
            AssertMemberType(NamedType.Function(new[] { Integer }, Integer), childScope, math, "Square");
            AssertMemberType(NamedType.Function(new[] { Integer }, Boolean), childScope, math, "Zero");

            Scope expectedFailure;
            rootScope.TryGetMemberScope(new NamedType("UnknownType"), out expectedFailure).ShouldBeFalse();
            expectedFailure.ShouldBeNull();
        }

        private static Class ParseClass(string classDeclaration)
        {
            var tokens = new RookLexer().Tokenize(classDeclaration);
            var parser = new RookGrammar().Class;
            return parser.Parse(new TokenStream(tokens)).Value;
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
            var var0 = new TypeVariable(0);
            var var1 = new TypeVariable(1);
            var var2 = new TypeVariable(2);
            var var3 = new TypeVariable(3);

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

        private static void AssertMemberType(DataType expectedType, Scope scope, NamedType typeKey, string memberKey)
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
    }
}