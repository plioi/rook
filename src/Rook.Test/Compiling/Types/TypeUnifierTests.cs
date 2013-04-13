using System.Collections.Generic;
using Should;

namespace Rook.Compiling.Types
{
    public class TypeUnifierTests
    {
        private static readonly NamedType Integer = NamedType.Integer;
        private static readonly NamedType Boolean = NamedType.Boolean;

        private readonly TypeUnifier unifier;
        private readonly TypeVariable x;
        private readonly TypeVariable y;
        private readonly TypeVariable z;

        public TypeUnifierTests()
        {
            unifier = new TypeUnifier();
            x = new TypeVariable(0);
            y = new TypeVariable(1);
            z = new TypeVariable(2);
        }

        private IEnumerable<string> Unify(DataType a, DataType b)
        {
            return unifier.Unify(a, b);
        }

        private DataType Normalize(DataType type)
        {
            return unifier.Normalize(type);
        }

        public void FailsToUnifyTypesWithDifferentNames()
        {
            var errors = Unify(Integer, Boolean);
            errors.ShouldList("Type mismatch: expected int, found bool.");
        }

        public void UnifiesSimpleNamedTypesWithThemselves()
        {
            var errors = Unify(Integer, Integer);
            errors.ShouldBeEmpty();
        }

        public void FailsToUnifyFunctionTypesWithDifferentArity()
        {
            var errors = Unify(Type("A", Integer, Boolean),
                               Type("A", Integer, Boolean, Boolean));
            errors.ShouldList("Type mismatch: expected A<int, bool>, found A<int, bool, bool>.");
        }

        public void FailsToUnifyCompoundTypesWithConflictingComponentTypes()
        {
            var errors = Unify(Type("A", Integer, Type("B", Integer)),
                               Type("A", Integer, Type("B", Boolean)));
            errors.ShouldList("Type mismatch: expected int, found bool.");
        }

        public void UnifiesCompoundTypesByRecursivelyUnifyingPairwiseComponentTypes()
        {
            var errors = Unify(Type("Foo", Integer, Boolean, Type("B", Integer)),
                               Type("Foo", Integer, Boolean, Type("B", Integer)));
            errors.ShouldBeEmpty();
        }

        public void NormalizesConcreteTypesByPerformingNoChanges()
        {
            Normalize(Integer).ShouldBeSameAs(Integer);

            var type = Type("A", Boolean);
            Normalize(type).ShouldBeSameAs(type);
        }

        public void NormalizesUnunifiedTypeVariablesByPerformingNoChanges()
        {
            Normalize(x).ShouldBeSameAs(x);

            var type = Type("A", x);
            Normalize(type).ShouldBeSameAs(type);
        }

        public void NormalizesUnifiedTypeVariablesByPerformingSubstitution()
        {
            var errorsA = Unify(x, Integer);
            var errorsB = Unify(Boolean, y);

            Normalize(x).ShouldBeSameAs(Integer);
            Normalize(y).ShouldBeSameAs(Boolean);
            Normalize(Type("A", x, y)).ShouldEqual(Type("A", Integer, Boolean));

            errorsA.ShouldBeEmpty();
            errorsB.ShouldBeEmpty();
        }

        public void UnunifiesTypeVariablesWithTheselves()
        {
            var errors = Unify(x, x);

            Normalize(x).ShouldBeSameAs(x);

            var type = Type("A", x);
            Normalize(type).ShouldBeSameAs(type);

            errors.ShouldBeEmpty();
        }

        public void FailsToUnifyRecursiveTypes()
        {
            var errors = Unify(x, Type("A", x));
            errors.ShouldList("Type mismatch: expected 0, found A<0>.");
        }

        public void SimplifiesChainsOfTypeVariablesIntroducedDuringUnification()
        {
            var errorsA = Unify(x, y);
            var errorsB = Unify(y, z);
            var errorsC = Unify(z, Integer);

            Normalize(x).ShouldBeSameAs(Integer);
            Normalize(y).ShouldBeSameAs(Integer);
            Normalize(z).ShouldBeSameAs(Integer);
            Normalize(Type("A", x, y, z)).ShouldEqual(Type("A", Integer, Integer, Integer));

            errorsA.ShouldBeEmpty();
            errorsB.ShouldBeEmpty();
            errorsC.ShouldBeEmpty();
        }

        public void FailsToUnifyTypeVariablesWithAConflictingPreviousUnification()
        {
            var errorsA = Unify(x, Integer);
            var errorsB = Unify(x, Boolean);

            errorsA.ShouldBeEmpty();
            errorsB.ShouldList("Type mismatch: expected int, found bool.");
        }

        public void UnifiesCircularChainsOfTypeVariablesByIgnoringRedundantRequests()
        {
            var errorsA = Unify(x, y);
            var errorsB = Unify(y, z);
            var errorsC = Unify(z, x);

            errorsA.ShouldBeEmpty();
            errorsB.ShouldBeEmpty();
            errorsC.ShouldBeEmpty();

            Normalize(x).ShouldBeSameAs(z);
            Normalize(y).ShouldBeSameAs(z);
            Normalize(z).ShouldBeSameAs(z);
        }

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

        private static NamedType Type(string name, params DataType[] genericArguments)
        {
            return new NamedType(name, genericArguments);
        }
    }
}