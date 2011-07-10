using System.Collections.Generic;
using NUnit.Framework;

namespace Rook.Compiling.Types
{
    [TestFixture]
    public class TypeNormalizerTests
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
            errors.ShouldList("Type mismatch: expected int, found bool.");
        }

        [Test]
        public void UnifiesSimpleNamedTypesWithThemselves()
        {
            var errors = Unify(Integer, Integer);
            errors.ShouldBeEmpty();
        }

        [Test]
        public void FailsToUnifyFunctionTypesWithDifferentArity()
        {
            var errors = Unify(Type("A", Integer, Boolean),
                               Type("A", Integer, Boolean, Boolean));
            errors.ShouldList("Type mismatch: expected A<int, bool>, found A<int, bool, bool>.");
        }

        [Test]
        public void FailsToUnifyCompoundTypesWithConflictingComponentTypes()
        {
            var errors = Unify(Type("A", Integer, Type("B", Integer)),
                               Type("A", Integer, Type("B", Boolean)));
            errors.ShouldList("Type mismatch: expected int, found bool.");
        }

        [Test]
        public void UnifiesCompoundTypesByRecursivelyUnifyingPairwiseComponentTypes()
        {
            var errors = Unify(Type("Foo", Integer, Boolean, Type("B", Integer)),
                               Type("Foo", Integer, Boolean, Type("B", Integer)));
            errors.ShouldBeEmpty();
        }

        [Test]
        public void NormalizesConcreteTypesByPerformingNoChanges()
        {
            Normalize(Integer).ShouldEqual(Integer);
            Normalize(Type("A", Boolean)).ShouldEqual(Type("A", Boolean));
        }

        [Test]
        public void NormalizesUnunifiedTypeVariablesByPerformingNoChanges()
        {
            Normalize(x).ShouldEqual(x);
            Normalize(Type("A", x)).ShouldEqual(Type("A", x));
        }

        [Test]
        public void NormalizesUnifiedTypeVariablesByPerformingSubstitution()
        {
            var errorsA = Unify(x, Integer);
            var errorsB = Unify(Boolean, y);

            Normalize(x).ShouldEqual(Integer);
            Normalize(y).ShouldEqual(Boolean);
            Normalize(Type("A", x, y)).ShouldEqual(Type("A", Integer, Boolean));

            errorsA.ShouldBeEmpty();
            errorsB.ShouldBeEmpty();
        }

        [Test]
        public void UnunifiesTypeVariablesWithTheselves()
        {
            var errors = Unify(x, x);

            Normalize(x).ShouldEqual(x);
            Normalize(Type("A", x)).ShouldEqual(Type("A", x));

            errors.ShouldBeEmpty();
        }

        [Test]
        public void FailsToUnifyRecursiveTypes()
        {
            var errors = Unify(x, Type("A", x));
            errors.ShouldList("Type mismatch: expected 0, found A<0>.");
        }

        [Test]
        public void SimplifiesChainsOfTypeVariablesIntroducedDuringUnification()
        {
            var errorsA = Unify(x, y);
            var errorsB = Unify(y, z);
            var errorsC = Unify(z, Integer);

            Normalize(x).ShouldEqual(Integer);
            Normalize(y).ShouldEqual(Integer);
            Normalize(z).ShouldEqual(Integer);
            Normalize(Type("A", x, y, z)).ShouldEqual(Type("A", Integer, Integer, Integer));

            errorsA.ShouldBeEmpty();
            errorsB.ShouldBeEmpty();
            errorsC.ShouldBeEmpty();
        }

        [Test]
        public void FailsToUnifyTypeVariablesWithAConflictingPreviousUnification()
        {
            var errorsA = Unify(x, Integer);
            var errorsB = Unify(x, Boolean);

            errorsA.ShouldBeEmpty();
            errorsB.ShouldList("Type mismatch: expected int, found bool.");
        }

        [Test]
        public void UnifiesCircularChainsOfTypeVariablesByIgnoringRedundantRequests()
        {
            var errorsA = Unify(x, y);
            var errorsB = Unify(y, z);
            var errorsC = Unify(z, x);

            errorsA.ShouldBeEmpty();
            errorsB.ShouldBeEmpty();
            errorsC.ShouldBeEmpty();

            Normalize(x).ShouldBeTheSameAs(z);
            Normalize(y).ShouldBeTheSameAs(z);
            Normalize(z).ShouldBeTheSameAs(z);
        }

        [Test]
        public void CollectsAllErrorsFoundDuringUnification()
        {
            var errorsA = Unify(Integer, Boolean);
            var errorsB = Unify(Boolean, Integer);
            var errorsC = Unify(Type("A", Integer), Type("B", Integer));
            var errorsD = Unify(Type("C", Type("D"), Type("E")), Type("C", Type("F"), Type("G")));

            errorsA.ShouldList("Type mismatch: expected int, found bool.");
            errorsB.ShouldList("Type mismatch: expected bool, found int.");
            errorsC.ShouldList("Type mismatch: expected A<int>, found B<int>.");
            errorsD.ShouldList("Type mismatch: expected D, found F.", "Type mismatch: expected E, found G.");
        }

        private static NamedType Type(string name, params DataType[] innerTypes)
        {
            return new NamedType(name, innerTypes);
        }
    }
}