using NUnit.Framework;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class BooleanLiteralTests : ExpressionTests
    {
        [Test]
        public void IsIdentifiedByKeywords()
        {
            Parses("true").IntoTree("true");
            Parses("false").IntoTree("false");
        }

        [Test]
        public void HasBooleanType()
        {
            AssertType(Boolean, "true");
            AssertType(Boolean, "false");
        }

        [Test]
        public void AreAlwaysFullyTyped()
        {
            var boolean = (BooleanLiteral) Parse("false");
            boolean.Type.ShouldEqual(Boolean);

            var typedBoolean = (BooleanLiteral) boolean.WithTypes(Environment()).Syntax;
            typedBoolean.Type.ShouldEqual(Boolean);
            typedBoolean.ShouldBeTheSameAs(boolean);
        }
    }
}