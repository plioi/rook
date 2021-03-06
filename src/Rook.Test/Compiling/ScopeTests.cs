﻿using System;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Should;

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

        public void StoresLocals()
        {
            AssertType(Integer, ab, "a");
            AssertType(Integer, ab, "b");

            AssertType(Boolean, cd, "c");
            AssertType(Boolean, cd, "d");
        }

        public void DefersToSurroundingScopeAfterSearchingLocals()
        {
            AssertType(Integer, cd, "a");
            AssertType(Integer, cd, "b");
        }

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
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, int, 0>", global, ReservedName.__index__);
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, int, int, Rook.Core.Collections.Vector<0>>", global, ReservedName.__slice__);
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, 0, Rook.Core.Collections.Vector<0>>", global, "Append");
            AssertType("System.Func<Rook.Core.Collections.Vector<0>, int, 0, Rook.Core.Collections.Vector<0>>", global, "With");
        }

        public void CanBePopulatedWithAUniqueBinding()
        {
            global.TryIncludeUniqueBinding("a", Integer).ShouldBeTrue();
            global.TryIncludeUniqueBinding(new StubBinding("b", Boolean)).ShouldBeTrue();
            AssertType(Integer, global, "a");
            AssertType(Boolean, global, "b");
        }

        public void DemandsUniqueBindingsWhenIncludingUniqueBindings()
        {
            global.TryIncludeUniqueBinding("a", Integer).ShouldBeTrue();
            global.TryIncludeUniqueBinding("a", Integer).ShouldBeFalse();
            global.TryIncludeUniqueBinding("a", Boolean).ShouldBeFalse();
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