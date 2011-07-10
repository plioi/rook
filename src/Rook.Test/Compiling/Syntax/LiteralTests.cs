using NUnit.Framework;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class LiteralTests : ExpressionTests
    {
        [Test]
        public void NullLiterals()
        {
            Parses("null").IntoTree("null");
            AssertType(NamedType.Nullable(new TypeVariable(17)), "null");
        }

        [Test]
        public void NullLiteralsCanCreateFullyTypedInstanceInTermsOfNewTypeVariable()
        {
            var node = (Null) Parse("null");
            node.Type.ShouldBeNull();

            var typedNode = (Null) node.WithTypes(Environment()).Syntax;
            typedNode.Type.ShouldEqual(NamedType.Nullable(new TypeVariable(17)));
        }

        [Test]
        public void BooleanLiterals()
        {
            Parses("true").IntoTree("true");
            Parses("false").IntoTree("false");

            AssertType(Boolean, "true");
            AssertType(Boolean, "false");
        }

        [Test]
        public void BooleanLiteralsAreAlwaysFullyTyped()
        {
            var boolean = (BooleanLiteral) Parse("false");
            boolean.Type.ShouldEqual(Boolean);

            var typedBoolean = (BooleanLiteral) boolean.WithTypes(Environment()).Syntax;
            typedBoolean.Type.ShouldEqual(Boolean);
            typedBoolean.ShouldEqual(boolean);
        }

        [Test]
        public void IntegerLiterals()
        {
            Parses("0").IntoTree("0");
            Parses("2147483647").IntoTree("2147483647");

            AssertType(Integer, "0");
            AssertType(Integer, "2147483647");
        }

        [Test]
        public void IntegerLiteralsAreAlwaysFullyTyped()
        {
            var integer = (IntegerLiteral) Parse("12345");
            integer.Type.ShouldEqual(Integer);

            var typedInteger = (IntegerLiteral) integer.WithTypes(Environment()).Syntax;
            typedInteger.Type.ShouldEqual(Integer);
            typedInteger.ShouldEqual(integer);
        }
    }
}