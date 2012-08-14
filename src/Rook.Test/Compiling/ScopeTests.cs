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

        private readonly Scope global, ab, cd;

        public ScopeTests()
        {
            using (TypeVariable.TestFactory())
            {
                global = new GlobalScope();

                ab = new LocalScope(global);
                ab.Bind("a", Integer);
                ab.Bind("b", Integer);

                cd = new LocalScope(ab);
                cd.Bind("c", Boolean);
                cd.Bind("d", Boolean);
            }
        }

        [Fact]
        public void StoresLocals()
        {
            AssertType(Integer, ab, "a");
            AssertType(Integer, ab, "b");

            AssertType(Boolean, cd, "c");
            AssertType(Boolean, cd, "d");
        }

        [Fact]
        public void DefersToSurroundingScopeAfterSearchingLocals()
        {
            AssertType(Integer, cd, "a");
            AssertType(Integer, cd, "b");
        }

        [Fact]
        public void ProvidesContainmentPredicate()
        {
            ab.Contains("a").ShouldBeTrue();
            ab.Contains("b").ShouldBeTrue();
            ab.Contains("c").ShouldBeFalse();
            ab.Contains("d").ShouldBeFalse();
            ab.Contains("z").ShouldBeFalse();

            cd.Contains("a").ShouldBeTrue();
            cd.Contains("b").ShouldBeTrue();
            cd.Contains("c").ShouldBeTrue();
            cd.Contains("d").ShouldBeTrue();
            cd.Contains("z").ShouldBeFalse();
        }

        [Fact]
        public void ProvidesBuiltinSignaturesInTheGlobalScope()
        {
            AssertType("System.Func<int, int, bool>", global, "<");
            AssertType("System.Func<int, int, bool>", global, "<=");
            AssertType("System.Func<int, int, bool>", global, ">");
            AssertType("System.Func<int, int, bool>", global, ">=");
            AssertType("System.Func<int, int, bool>", global, "==");
            AssertType("System.Func<int, int, bool>", global, "!=");

            AssertType("System.Func<int, int, int>", global, "+");
            AssertType("System.Func<int, int, int>", global, "-");
            AssertType("System.Func<int, int, int>", global, "*");
            AssertType("System.Func<int, int, int>", global, "/");

            AssertType("System.Func<bool, bool, bool>", global, "||");
            AssertType("System.Func<bool, bool, bool>", global, "&&");
            AssertType("System.Func<bool, bool>", global, "!");

            AssertType("System.Func<Rook.Core.Nullable<0>, 0, 0>", global, "??");
            AssertType("System.Func<0, Rook.Core.Void>", global, "Print");
            AssertType("System.Func<0, Rook.Core.Nullable<0>>", global, "Nullable");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, 0>", global, "First");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, int, System.Collections.Generic.IEnumerable<0>>", global, "Take");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, int, System.Collections.Generic.IEnumerable<0>>", global, "Skip");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, bool>", global, "Any");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, int>", global, "Count");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, System.Func<0, 1>, System.Collections.Generic.IEnumerable<1>>", global, "Select");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<0>, System.Func<0, bool>, System.Collections.Generic.IEnumerable<0>>", global, "Where");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, System.Collections.Generic.IEnumerable<0>>", global, "Each");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, int, 0>", global, "Index");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, int, int, Rook.Core.Collections.Vector<0>>", global, "Slice");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, 0, Rook.Core.Collections.Vector<0>>", global, "Append");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, int, 0, Rook.Core.Collections.Vector<0>>", global, "With");
        }

        [Fact]
        public void CanBePopulatedWithAUniqueBinding()
        {
            global.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")).ShouldBeTrue();
            global.TryIncludeUniqueBinding(new Parameter(null, Boolean, "b")).ShouldBeTrue();
            AssertType(Integer, global, "a");
            AssertType(Boolean, global, "b");
        }

        [Fact]
        public void DemandsUniqueBindingsWhenIncludingUniqueBindings()
        {
            global.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")).ShouldBeTrue();
            global.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")).ShouldBeFalse();
            global.TryIncludeUniqueBinding(new Parameter(null, Boolean, "a")).ShouldBeFalse();
            AssertType(Integer, global, "a");
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