using Should;

namespace Rook.Compiling.Syntax
{
    [Facts]
    public class BooleanLiteralTests : ExpressionTests
    {
        public void IsIdentifiedByKeywords()
        {
            Parses("true").IntoTree("true");
            Parses("false").IntoTree("false");
        }

        public void HasBooleanType()
        {
            Type("true").ShouldEqual(Boolean);
            Type("false").ShouldEqual(Boolean);
        }

        public void AreAlwaysFullyTyped()
        {
            var boolean = (BooleanLiteral) Parse("false");
            boolean.Type.ShouldEqual(Boolean);

            var typedBoolean = WithTypes(boolean);
            typedBoolean.Type.ShouldEqual(Boolean);
            typedBoolean.ShouldBeSameAs(boolean);
        }
    }
}