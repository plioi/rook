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
            AssertType(NamedType.Nullable(new TypeVariable(17)), "null");
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var node = (Null)Parse("null");
            node.Type.ShouldBeNull();

            var typedNode = (Null)node.WithTypes(Environment()).Syntax;
            typedNode.Type.ShouldEqual(NamedType.Nullable(new TypeVariable(17)));
        }
    }
}