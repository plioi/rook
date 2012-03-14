using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class BooleanLiteralTests : ExpressionTests
    {
        [Fact]
        public void IsIdentifiedByKeywords()
        {
            Parses("true").IntoTree("true");
            Parses("false").IntoTree("false");
        }

        [Fact]
        public void HasBooleanType()
        {
            AssertType(Boolean, "true");
            AssertType(Boolean, "false");
        }

        [Fact]
        public void AreAlwaysFullyTyped()
        {
            var boolean = (BooleanLiteral) Parse("false");
            boolean.Type.ShouldEqual(Boolean);

            var typedBoolean = (BooleanLiteral) boolean.WithTypes(Environment()).Syntax;
            typedBoolean.Type.ShouldEqual(Boolean);
            typedBoolean.ShouldBeSameAs(boolean);
        }
    }
}