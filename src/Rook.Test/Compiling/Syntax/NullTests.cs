using Rook.Compiling.Types;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class NullTests : ExpressionTests
    {
        [Fact]
        public void IsIdentifiedByKeyword()
        {
            Parses("null").IntoTree("null");
        }

        [Fact]
        public void HasUniqueTypeVariableAsItsType()
        {
            Type("null").ShouldEqual(NamedType.Nullable(new TypeVariable(2)));
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var node = (Null)Parse("null");
            node.Type.ShouldBeNull();

            var typedNode = (Null)node.WithTypes(Scope()).Syntax;
            typedNode.Type.ShouldEqual(NamedType.Nullable(new TypeVariable(2)));
        }
    }
}