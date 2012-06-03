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
            var @null = (Null)Parse("null");
            @null.Type.ShouldBeNull();

            var typedNull = WithTypes(@null);
            typedNull.Type.ShouldEqual(NamedType.Nullable(new TypeVariable(2)));
        }
    }
}