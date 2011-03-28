using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Rook.Compiling.Types
{
    [TestFixture]
    public sealed class TypeNormalizerSpec
    {
        private static readonly NamedType Integer = NamedType.Integer;
        private static readonly NamedType Boolean = NamedType.Boolean;

        private TypeNormalizer normalizer;
        private TypeVariable x;
        private TypeVariable y;
        private TypeVariable z;

        [SetUp]
        public void SetUp()
        {
            x = new TypeVariable(0);
            y = new TypeVariable(1);
            z = new TypeVariable(2);
            normalizer = new TypeNormalizer();
        }

        private IEnumerable<string> Unify(DataType a, DataType b)
        {
            return normalizer.Unify(a, b);
        }

        private DataType Normalize(DataType type)
        {
            return normalizer.Normalize(type);
        }

        [Test]
        public void FailsToUnifyTypesWithDifferentNames()
        {
            var errors = Unify(Integer, Boolean);
            Assert.AreEqual(new[] {"Type mismatch: expected int, found bool."}, errors.ToArray());
        }

        [Test]
        public void UnifiesSimpleNamedTypesWithThemselves()
        {
            var errors = Unify(Integer, Integer);
            Assert.IsEmpty(errors.ToArray());
        }

        [Test]
        public void FailsToUnifyFunctionTypesWithDifferentArity()
        {
            var errors = Unify(Type("A", Integer, Boolean),
                               Type("A", Integer, Boolean, Boolean));
            Assert.AreEqual(new[] {"Type mismatch: expected A<int, bool>, found A<int, bool, bool>."}, errors.ToArray());
        }

        [Test]
        public void FailsToUnifyCompoundTypesWithConflictingComponentTypes()
        {
            var errors = Unify(Type("A", Integer, Type("B", Integer)),
                               Type("A", Integer, Type("B", Boolean)));
            Assert.AreEqual(new[] {"Type mismatch: expected int, found bool."}, errors.ToArray());
        }

        [Test]
        public void UnifiesCompoundTypesByRecursivelyUnifyingPairwiseComponentTypes()
        {
            var errors = Unify(Type("Foo", Integer, Boolean, Type("B", Integer)),
                               Type("Foo", Integer, Boolean, Type("B", Integer)));
            Assert.IsEmpty(errors.ToArray());
        }

        [Test]
        public void NormalizesConcreteTypesByPerformingNoChanges()
        {
            Assert.AreSame(Integer, Normalize(Integer));
            Assert.AreSame(Type("A", Boolean), Normalize(Type("A", Boolean)));
        }

        [Test]
        public void NormalizesUnunifiedTypeVariablesByPerformingNoChanges()
        {
            Assert.AreSame(x, Normalize(x));
            Assert.AreSame(Type("A", x), Normalize(Type("A", x)));
        }

        [Test]
        public void NormalizesUnifiedTypeVariablesByPerformingSubstitution()
        {
            var errorsA = Unify(x, Integer);
            var errorsB = Unify(Boolean, y);

            Assert.AreSame(Integer, Normalize(x));
            Assert.AreSame(Boolean, Normalize(y));
            Assert.AreSame(Type("A", Integer, Boolean), Normalize(Type("A", x, y)));

            Assert.IsEmpty(errorsA.ToArray());
            Assert.IsEmpty(errorsB.ToArray());
        }

        [Test]
        public void UnunifiesTypeVariablesWithTheselves()
        {
            var errors = Unify(x, x);

            Assert.AreSame(x, Normalize(x));
            Assert.AreSame(Type("A", x), Normalize(Type("A", x)));

            Assert.IsEmpty(errors.ToArray());
        }

        [Test]
        public void FailsToUnifyRecursiveTypes()
        {
            var errors = Unify(x, Type("A", x));
            Assert.AreEqual(new[] {"Type mismatch: expected 0, found A<0>."}, errors.ToArray());
        }

        [Test]
        public void SimplifiesChainsOfTypeVariablesIntroducedDuringUnification()
        {
            var errorsA = Unify(x, y);
            var errorsB = Unify(y, z);
            var errorsC = Unify(z, Integer);
            
            Assert.AreSame(Integer, Normalize(x));
            Assert.AreSame(Integer, Normalize(y));
            Assert.AreSame(Integer, Normalize(z));
            Assert.AreSame(Type("A", Integer, Integer, Integer), Normalize(Type("A", x, y, z)));

            Assert.IsEmpty(errorsA.ToArray());
            Assert.IsEmpty(errorsB.ToArray());
            Assert.IsEmpty(errorsC.ToArray());
        }

        [Test]
        public void FailsToUnifyTypeVariablesWithAConflictingPreviousUnification()
        {
            var errorsA = Unify(x, Integer);
            var errorsB = Unify(x, Boolean);

            Assert.IsEmpty(errorsA.ToArray());
            Assert.AreEqual(new[] {"Type mismatch: expected int, found bool."}, errorsB.ToArray());
        }

        [Test]
        public void UnifiesCircularChainsOfTypeVariablesByIgnoringRedundantRequests()
        {
            var errorsA = Unify(x, y);
            var errorsB = Unify(y, z);
            var errorsC = Unify(z, x);

            Assert.IsEmpty(errorsA.ToArray());
            Assert.IsEmpty(errorsB.ToArray());
            Assert.IsEmpty(errorsC.ToArray());

            Assert.AreSame(z, Normalize(x));
            Assert.AreSame(z, Normalize(y));
            Assert.AreSame(z, Normalize(z));
        }

        [Test]
        public void CollectsAllErrorsFoundDuringUnification()
        {
            var errorsA = Unify(Integer, Boolean);
            var errorsB = Unify(Boolean, Integer);
            var errorsC = Unify(Type("A", Integer), Type("B", Integer));
            var errorsD = Unify(Type("C", Type("D"), Type("E")), Type("C", Type("F"), Type("G")));

            Assert.AreEqual(new[] { "Type mismatch: expected int, found bool." }, errorsA.ToArray());
            Assert.AreEqual(new[] { "Type mismatch: expected bool, found int." }, errorsB.ToArray());
            Assert.AreEqual(new[] { "Type mismatch: expected A<int>, found B<int>." }, errorsC.ToArray());
            Assert.AreEqual(new[] {
                                    "Type mismatch: expected D, found F.",
                                    "Type mismatch: expected E, found G."
                                  }, errorsD.ToArray());
        }

        private static NamedType Type(string name, params DataType[] innerTypes)
        {
            return NamedType.Create(name, innerTypes);
        }
    }
}