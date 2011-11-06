using NUnit.Framework;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class NullTests : ExpressionTests
    {
        [Test]
        public void IsIdentifiedByKeyword()
        {
            Parses("null").IntoTree("null");
        }

        [Test]
        public void HasUniqueTypeVariableAsItsType()
        {
            AssertType(NamedType.Nullable(new TypeVariable(17)), "null");
        }

        [Test]
        public void CanCreateFullyTypedInstance()
        {
            var node = (Null)Parse("null");
            node.Type.ShouldBeNull();

            var typedNode = (Null)node.WithTypes(Environment()).Syntax;
            typedNode.Type.ShouldEqual(NamedType.Nullable(new TypeVariable(17)));
        }
    }
}