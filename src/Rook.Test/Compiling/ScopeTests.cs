using System;
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
            root = Scope.CreateRoot(typeChecker);

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

            AssertType("System.Func<Rook.Core.Nullable<2>, 2, 2>", root, "??");
            AssertType("System.Func<3, Rook.Core.Void>", root, "Print");
            AssertType("System.Func<4, Rook.Core.Nullable<4>>", root, "Nullable");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<5>, 5>", root, "First");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<6>, int, System.Collections.Generic.IEnumerable<6>>", root, "Take");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<7>, int, System.Collections.Generic.IEnumerable<7>>", root, "Skip");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<8>, bool>", root, "Any");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<9>, int>", root, "Count");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<10>, System.Func<10, 11>, System.Collections.Generic.IEnumerable<11>>", root, "Select");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<12>, System.Func<12, bool>, System.Collections.Generic.IEnumerable<12>>", root, "Where");
            AssertType("System.Func<Rook.Core.Collections.Vector<13>, System.Collections.Generic.IEnumerable<13>>", root, "Each");
            AssertType("System.Func<Rook.Core.Collections.Vector<14>, int, 14>", root, "Index");
            AssertType("System.Func<Rook.Core.Collections.Vector<15>, int, int, Rook.Core.Collections.Vector<15>>", root, "Slice");
            AssertType("System.Func<Rook.Core.Collections.Vector<16>, 16, Rook.Core.Collections.Vector<16>>", root, "Append");
            AssertType("System.Func<Rook.Core.Collections.Vector<17>, int, 17, Rook.Core.Collections.Vector<17>>", root, "With");
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

            var outerLambda = bc.CreateLambdaScope();
            var local = outerLambda.CreateLocalScope();
            var middleLambda = local.CreateLambdaScope();
            var local2 = middleLambda.CreateLocalScope();
            var innerLambda = local2.CreateLambdaScope();

            outerLambda.TreatAsNonGeneric(new[] { var0 });
            middleLambda.TreatAsNonGeneric(new[] { var1, var2 });
            innerLambda.TreatAsNonGeneric(new[] { var3 });

            outerLambda.IsGeneric(var0).ShouldBeFalse();
            outerLambda.IsGeneric(var1).ShouldBeTrue();
            outerLambda.IsGeneric(var2).ShouldBeTrue();
            outerLambda.IsGeneric(var3).ShouldBeTrue();

            middleLambda.IsGeneric(var0).ShouldBeFalse();
            middleLambda.IsGeneric(var1).ShouldBeFalse();
            middleLambda.IsGeneric(var2).ShouldBeFalse();
            middleLambda.IsGeneric(var3).ShouldBeTrue();

            innerLambda.IsGeneric(var0).ShouldBeFalse();
            innerLambda.IsGeneric(var1).ShouldBeFalse();
            innerLambda.IsGeneric(var2).ShouldBeFalse();
            innerLambda.IsGeneric(var3).ShouldBeFalse();
        }

        [Fact]
        public void FreshensTypeVariablesOnEachLookup()
        {
            var scope = Scope.CreateRoot(new TypeChecker());

            scope["concreteType"] = Integer;
            AssertType(Integer, scope, "concreteType");

            scope["typeVariable"] = new TypeVariable(0);
            AssertType(new TypeVariable(2), scope, "typeVariable");
            AssertType(new TypeVariable(3), scope, "typeVariable");

            var expectedTypeAfterLookup = new NamedType("A", new TypeVariable(4), new TypeVariable(5), new NamedType("B", new TypeVariable(4), new TypeVariable(5)));
            var definedType = new NamedType("A", new TypeVariable(0), new TypeVariable(1), new NamedType("B", new TypeVariable(0), new TypeVariable(1)));
            scope["genericTypeIncludingTypeVariables"] = definedType;
            AssertType(expectedTypeAfterLookup, scope, "genericTypeIncludingTypeVariables");
        }

        [Fact]
        public void FreshensOnlyGenericTypeVariablesOnEachLookup()
        {
            //Prevents type '1' from being freshened on type lookup by marking it as non-generic in the scope:

            var expectedTypeAfterLookup = new NamedType("A", new TypeVariable(2), new TypeVariable(1), new NamedType("B", new TypeVariable(2), new TypeVariable(1)));
            var definedType = new NamedType("A", new TypeVariable(0), new TypeVariable(1), new NamedType("B", new TypeVariable(0), new TypeVariable(1)));

            var scope = Scope.CreateRoot(new TypeChecker());
            var lambdaScope = scope.CreateLambdaScope();
            lambdaScope.TreatAsNonGeneric(new[] { new TypeVariable(1) });
            lambdaScope["genericTypeIncludingGenericAndNonGenericTypeVariables"] = definedType;

            AssertType(expectedTypeAfterLookup, lambdaScope, "genericTypeIncludingGenericAndNonGenericTypeVariables");
        }

        private static void AssertType(DataType expectedType, Scope scope, string key)
        {
            DataType value;

            if (scope.TryGet(key, out value))
                value.ShouldEqual(expectedType);
            else
                throw new Exception("Failed to look up the type of '" + key + "' in the Scope");
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