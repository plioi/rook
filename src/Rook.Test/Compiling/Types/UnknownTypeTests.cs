using System.Collections.Generic;
using Should;
using Xunit;

namespace Rook.Compiling.Types
{
    public class UnknownTypeTests
    {
        private static readonly UnknownType Unknown = UnknownType.Instance;

        [Fact]
        public void HasAName()
        {
            Unknown.Name.ShouldEqual("?");
        }

        [Fact]
        public void HasAStringRepresentation()
        {
            Unknown.ToString().ShouldEqual("?");
        }

        [Fact]
        public void HasZeroGenericArguments()
        {
            Unknown.GenericArguments.ShouldBeEmpty();
        }

        [Fact]
        public void IsNotGeneric()
        {
            Unknown.IsGeneric.ShouldBeFalse();
        }

        [Fact]
        public void IsNotAGenericTypeDefinition()
        {
            Unknown.IsGenericTypeDefinition.ShouldBeFalse();
        }

        [Fact]
        public void DoesNotContainTypeVariables()
        {
            Unknown.Contains(new TypeVariable(0)).ShouldBeFalse();
            Unknown.FindTypeVariables().ShouldBeEmpty();
        }

        [Fact]
        public void TypeVariableSubstitutionIsAnIdentityOperation()
        {
            var substitutions = new Dictionary<TypeVariable, DataType> { { new TypeVariable(0), NamedType.Integer } };

            Unknown.ReplaceTypeVariables(substitutions).ShouldEqual(Unknown);
        }

        [Fact]
        public void HasValueEqualitySemantics()
        {
            Unknown.ShouldEqual(Unknown);
            Unknown.ShouldNotEqual((DataType)new TypeVariable(0));
            Unknown.ShouldNotEqual((DataType)NamedType.Integer);
            Unknown.GetHashCode().ShouldNotEqual(NamedType.Integer.GetHashCode());
        }
    }
}