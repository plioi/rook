using NUnit.Framework;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling
{
    [TestFixture]
    public class EnvironmentSpec
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
            Assert.IsTrue(ab.Contains("a"));
            Assert.IsTrue(ab.Contains("b"));
            Assert.IsFalse(ab.Contains("c"));
            Assert.IsFalse(ab.Contains("z"));

            Assert.IsTrue(bc.Contains("a"));
            Assert.IsTrue(bc.Contains("b"));
            Assert.IsTrue(bc.Contains("c"));
            Assert.IsFalse(bc.Contains("z"));
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
            Assert.AreEqual(new TypeVariable(17), root.CreateTypeVariable());
            Assert.AreEqual(new TypeVariable(18), root.CreateTypeVariable());
            Assert.AreEqual(new TypeVariable(19), ab.CreateTypeVariable());
            Assert.AreEqual(new TypeVariable(20), bc.CreateTypeVariable());
            Assert.AreEqual(new TypeVariable(21), ab.CreateTypeVariable());
            Assert.AreEqual(new TypeVariable(22), bc.CreateTypeVariable());
        }

        [Test]
        public void ProvidesTypeNormalizerSharedWithAllLocalEnvironments()
        {
            Assert.AreSame(root.TypeNormalizer, ab.TypeNormalizer);
            Assert.AreSame(ab.TypeNormalizer, bc.TypeNormalizer);
            Assert.AreNotSame(root.TypeNormalizer, new Environment().TypeNormalizer);
        }

        [Test]
        public void CanBePopulatedWithAUniqueBinding()
        {
            Assert.IsTrue(root.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")));
            Assert.IsTrue(root.TryIncludeUniqueBinding(new Parameter(null, Boolean, "b")));
            AssertType(Integer, root, "a");
            AssertType(Boolean, root, "b");
        }

        [Test]
        public void DemandsUniqueBindingsWhenIncludingUniqueBindings()
        {
            Assert.IsTrue(root.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")));
            Assert.IsFalse(root.TryIncludeUniqueBinding(new Parameter(null, Integer, "a")));
            Assert.IsFalse(root.TryIncludeUniqueBinding(new Parameter(null, Boolean, "a")));
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
            
            Assert.IsFalse(root.IsGeneric(var0));
            Assert.IsTrue(root.IsGeneric(var1));
            Assert.IsTrue(root.IsGeneric(var2));
            Assert.IsTrue(root.IsGeneric(var3));

            Assert.IsFalse(ab.IsGeneric(var0));
            Assert.IsFalse(ab.IsGeneric(var1));
            Assert.IsFalse(ab.IsGeneric(var2));
            Assert.IsTrue(ab.IsGeneric(var3));

            Assert.IsFalse(bc.IsGeneric(var0));
            Assert.IsFalse(bc.IsGeneric(var1));
            Assert.IsFalse(bc.IsGeneric(var2));
            Assert.IsFalse(bc.IsGeneric(var3));
        }

        private static void AssertType(DataType expectedType, Environment environment, string key)
        {
            DataType value;

            if (environment.TryGet(key, out value))
                Assert.AreSame(expectedType, value);
            else
                Assert.Fail();
        }

        private static void AssertType(string expectedType, Environment environment, string key)
        {
            DataType value;

            if (environment.TryGet(key, out value))
                Assert.AreEqual(expectedType, value.ToString());
            else
                Assert.Fail();
        }
    }
}