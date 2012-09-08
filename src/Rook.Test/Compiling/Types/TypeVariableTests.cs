using System.Collections.Generic;
using Should;

namespace Rook.Compiling.Types
{
    [Facts]
    public class TypeVariableTests
    {
        private readonly TypeVariable a;
        private readonly TypeVariable b;

        public TypeVariableTests()
        {
            a = new TypeVariable(0);
            b = new TypeVariable(1);
        }

        public void HasAName()
        {
            a.Name.ShouldEqual("0");
            b.Name.ShouldEqual("1");
        }

        public void HasZeroGenericArguments()
        {
            a.GenericArguments.ShouldBeEmpty();
            b.GenericArguments.ShouldBeEmpty();
        }

        public void CanBeEitherGenericOrNonGeneric()
        {
            TypeVariable.CreateGeneric().IsGeneric.ShouldBeTrue();
            TypeVariable.CreateNonGeneric().IsGeneric.ShouldBeFalse();
        }

        public void CanNeverBeAGenericTypeDefinition()
        {
            TypeVariable.CreateGeneric().IsGenericTypeDefinition.ShouldBeFalse();
            TypeVariable.CreateNonGeneric().IsGenericTypeDefinition.ShouldBeFalse();
        }

        public void ContainsOnlyItself()
        {
            a.Contains(a).ShouldBeTrue();
            b.Contains(b).ShouldBeTrue();

            a.Contains(b).ShouldBeFalse();
            b.Contains(a).ShouldBeFalse();
        }

        public void YieldsOnlyItselfWhenAskedToFindTypeVariableOccurrences()
        {
            a.FindTypeVariables().ShouldList(a);
            b.FindTypeVariables().ShouldList(b);
        }

        public void CanPerformTypeVariableSubstitutionOnItself()
        {
            var replaceAWithInteger = new Dictionary<TypeVariable, DataType> { { a, NamedType.Integer } };

            a.ReplaceTypeVariables(replaceAWithInteger).ShouldEqual(NamedType.Integer);
            b.ReplaceTypeVariables(replaceAWithInteger).ShouldEqual(b);
        }

        public void HasValueEqualitySemantics()
        {
            a.ShouldEqual(a);
            a.ShouldEqual(new TypeVariable(0));
            a.ShouldNotEqual(b);
            a.ShouldNotEqual((DataType)new NamedType("A"));

            a.GetHashCode().ShouldEqual(new TypeVariable(0).GetHashCode());
            a.GetHashCode().ShouldNotEqual(b.GetHashCode());
        }

        public void HasFactoryThatProvidesStreamOfUniqueTypeVariables()
        {
            var x = TypeVariable.CreateGeneric();
            var y = TypeVariable.CreateGeneric();

            ulong xName = ulong.Parse(x.Name);

            x.ShouldEqual(new TypeVariable(xName));
            y.ShouldEqual(new TypeVariable(xName + 1));
            TypeVariable.CreateGeneric().ShouldEqual(new TypeVariable(xName + 2));
            TypeVariable.CreateNonGeneric().ShouldEqual(new TypeVariable(xName + 3, false));
            TypeVariable.CreateGeneric().ShouldEqual(new TypeVariable(xName + 4));
        }

        public void HasFactoryThatCanBeTemporarilyReplacedForTestingPursposes()
        {
            using (TypeVariable.TestFactory())
            {
                var x = TypeVariable.CreateGeneric();
                var y = TypeVariable.CreateGeneric();

                x.ShouldEqual(new TypeVariable(0));
                y.ShouldEqual(new TypeVariable(1));
                TypeVariable.CreateGeneric().ShouldEqual(new TypeVariable(2));
                TypeVariable.CreateNonGeneric().ShouldEqual(new TypeVariable(3, false));
                TypeVariable.CreateGeneric().ShouldEqual(new TypeVariable(4));
            }
        }
    }
}