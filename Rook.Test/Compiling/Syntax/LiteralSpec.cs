using NUnit.Framework;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class LiteralSpec : ExpressionSpec
    {
        [Test]
        public void NullLiterals()
        {
            AssertTree("null", "null");
            AssertType(NamedType.Nullable(new TypeVariable(17)), "null");
        }

        [Test]
        public void NullLiteralsCanCreateFullyTypedInstanceInTermsOfNewTypeVariable()
        {
            Null node = (Null) Parse("null");
            Assert.IsNull(node.Type);

            Null typedNode = (Null) node.WithTypes(Environment()).Syntax;
            Assert.AreSame(NamedType.Nullable(new TypeVariable(17)), typedNode.Type);
        }

        [Test]
        public void BooleanLiterals()
        {
            AssertTree("true", "true");
            AssertTree("false", "false");

            AssertType(Boolean, "true");
            AssertType(Boolean, "false");
        }

        [Test]
        public void BooleanLiteralsAreAlwaysFullyTyped()
        {
            BooleanLiteral boolean = (BooleanLiteral) Parse("false");
            Assert.AreSame(Boolean, boolean.Type);

            BooleanLiteral typedBoolean = (BooleanLiteral) boolean.WithTypes(Environment()).Syntax;
            Assert.AreSame(Boolean, typedBoolean.Type);
            Assert.AreSame(boolean, typedBoolean);
        }

        [Test]
        public void IntegerLiterals()
        {
            AssertTree("0", "0");
            AssertTree("2147483647", "2147483647");

            AssertType(Integer, "0");
            AssertType(Integer, "2147483647");
        }

        [Test]
        public void IntegerLiteralsAreAlwaysFullyTyped()
        {
            IntegerLiteral integer = (IntegerLiteral) Parse("12345");
            Assert.AreSame(Integer, integer.Type);

            IntegerLiteral typedInteger = (IntegerLiteral) integer.WithTypes(Environment()).Syntax;
            Assert.AreSame(Integer, typedInteger.Type);
            Assert.AreSame(integer, typedInteger);
        }
    }
}