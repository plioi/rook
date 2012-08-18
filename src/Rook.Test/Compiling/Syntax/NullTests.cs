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
            using (TypeVariable.TestFactory())
            {
                Type("null").ShouldEqual(NamedType.Nullable(new TypeVariable(2)));
            }
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            using (TypeVariable.TestFactory())
            {
                var @null = (Null) Parse("null");
                @null.Type.ShouldEqual(Unknown);

                var typedNull = WithTypes(@null);
                typedNull.Type.ShouldEqual(NamedType.Nullable(new TypeVariable(2)));
            }
        }
    }
}