using System.Collections.Generic;
using Should;

namespace Rook.Compiling.Types
{
    public class UnknownTypeTests
    {
        private static readonly UnknownType Unknown = UnknownType.Instance;

        public void HasAName()
        {
            Unknown.Name.ShouldEqual("?");
        }

        public void HasAStringRepresentation()
        {
            Unknown.ToString().ShouldEqual("?");
        }

        public void HasZeroGenericArguments()
        {
            Unknown.GenericArguments.ShouldBeEmpty();
        }

        public void IsNotGeneric()
        {
            Unknown.IsGeneric.ShouldBeFalse();
        }

        public void IsNotAGenericTypeDefinition()
        {
            Unknown.IsGenericTypeDefinition.ShouldBeFalse();
        }

        public void DoesNotContainTypeVariables()
        {
            Unknown.Contains(new TypeVariable(0)).ShouldBeFalse();
            Unknown.FindTypeVariables().ShouldBeEmpty();
        }

        public void TypeVariableSubstitutionIsAnIdentityOperation()
        {
            var substitutions = new Dictionary<TypeVariable, DataType> { { new TypeVariable(0), NamedType.Integer } };

            Unknown.ReplaceTypeVariables(substitutions).ShouldEqual(Unknown);
        }

        public void HasValueEqualitySemantics()
        {
            Unknown.ShouldEqual(Unknown);
            Unknown.ShouldNotEqual((DataType)new TypeVariable(0));
            Unknown.ShouldNotEqual((DataType)NamedType.Integer);
            Unknown.GetHashCode().ShouldNotEqual(NamedType.Integer.GetHashCode());
        }
    }
}