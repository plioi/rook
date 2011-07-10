using NUnit.Framework;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling
{
    [TestFixture]
    public class EnvironmentTests
    {
        private static readonly NamedType Integer = NamedType.Integer;
        private static readonly NamedType Boolean = NamedType.Boolean;

        private Environment root, ab, bc;

        [SetUp]
        public void SetUp()
        {
            root = new Environment();

            ab = new Environment(root);
            ab["a"] = Integer;
            ab["b"] = Integer;

            bc = new Environment(ab);
            bc["b"] = Boolean;
            bc["c"] = Boolean;
        }

        [Test]
        public void StoresLocals()
        {
            AssertType(Integer, ab, "a");
            AssertType(Integer, ab, "b");
        }

        [Test]
        public void DefersToSurroundingScopeAfterSearchingLocals()
        {
            AssertType(Integer, bc, "a");
            AssertType(Boolean, bc, "b");
            AssertType(Boolean, bc, "c");
        }

        [Test]
        public void AllowsOverwritingInsideLocals()
        {
            ab["b"] = Boolean;
            bc["b"] = Integer;

            AssertType(Boolean, ab, "b");
            AssertType(Integer, bc, "b");
        }

        [Test]
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

        [Test]
        public void ProvidesPrimitiveSignatures()
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
            AssertType("System.Func<1, Rook.Core.Void>", root, "Print");
            AssertType("System.Func<2, Rook.Core.Nullable<2>>", root, "Nullable");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<3>, 3>", root, "First");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<4>, int, System.Collections.Generic.IEnumerable<4>>", root, "Take");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<5>, int, System.Collections.Generic.IEnumerable<5>>", root, "Skip");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<6>, bool>", root, "Any");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<7>, int>", root, "Count");
            AssertType("System.Func<System.Collections.Generic.IEnumerable<8>, System.Func<8, 9>, System.Collections.Generic.IEnumerable<9>>", root, "Select");
            AssertType("System.Func<11, System.Collections.Generic.IEnumerable<11>, System.Collections.Generic.IEnumerable<11>>", root, "Yield");
            AssertType("System.Func<Rook.Core.Collections.Vector<12>, System.Collections.Generic.IEnumerable<12>>", root, "Each");
            AssertType("System.Func<Rook.Core.Collections.Vector<13>, int, 13>", root, "Index");
            AssertType("System.Func<Rook.Core.Collections.Vector<14>, int, int, Rook.Core.Collections.Vector<14>>", root, "Slice");
            AssertType("System.Func<Rook.Core.Collections.Vector<15>, 15, Rook.Core.Collections.Vector<15>>", root, "Append");
            AssertType("System.Func<Rook.Core.Collections.Vector<16>, int, 16, Rook.Core.Collections.Vector<16>>", root, "With");
        }

        [Test]
        public void ProvidesStreamOfUniqueTypeVariables()
        {
            root.CreateTypeVariable().ShouldEqual(new TypeVariable(17));
            root.CreateTypeVariable().ShouldEqual(new TypeVariable(18));
            ab.CreateTypeVariable().ShouldEqual(new TypeVariable(19));
            bc.CreateTypeVariable().ShouldEqual(new TypeVariable(20));
            ab.CreateTypeVariable().ShouldEqual(new TypeVariable(21));
            bc.CreateTypeVariable().ShouldEqual(new TypeVariable(22));
        }

        [Test]
        public void ProvidesTypeNormalizerSharedWithAllLocalEnvironments()
        {
            ab.TypeNormalizer.ShouldBeTheSameAs(root.TypeNormalizer);
            bc.TypeNormalizer.ShouldBeTheSameAs(ab.TypeNormalizer);
            new Environment().TypeNormalizer.ShouldNotBeTheSameAs(root.TypeNormalizer);
        }

        [Test]
        public void CanBePopulatedWithAUniqueBinding()
        {
            root.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")).ShouldBeTrue();
            root.TryIncludeUniqueBinding(new Parameter(null, Boolean, "b")).ShouldBeTrue();
            AssertType(Integer, root, "a");
            AssertType(Boolean, root, "b");
        }

        [Test]
        public void DemandsUniqueBindingsWhenIncludingUniqueBindings()
        {
            root.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")).ShouldBeTrue();
            root.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")).ShouldBeFalse();
            root.TryIncludeUniqueBinding(new Parameter(null, Boolean, "a")).ShouldBeFalse();
            AssertType(Integer, root, "a");
        }

        [Test]
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
                value.ShouldBeTheSameAs(expectedType);
            else
                Assert.Fail();
        }

        private static void AssertType(string expectedType, Environment environment, string key)
        {
            DataType value;

            if (environment.TryGet(key, out value))
                expectedType.ShouldEqual(value.ToString());
            else
                Assert.Fail();
        }
    }
}