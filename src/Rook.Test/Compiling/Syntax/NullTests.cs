using Rook.Compiling.Types;
using Should;

namespace Rook.Compiling.Syntax
{
    public class NullTests : ExpressionTests
    {
        public void IsIdentifiedByKeyword()
        {
            Parses("null").IntoTree("null");
        }

        public void HasUniqueTypeVariableAsItsType()
        {
            using (TypeVariable.TestFactory())
            {
                Type("null").ShouldEqual(NamedType.Nullable(new TypeVariable(6)));
            }
        }

        public void CanCreateFullyTypedInstance()
        {
            using (TypeVariable.TestFactory())
            {
                var @null = (Null) Parse("null");
                @null.Type.ShouldEqual(Unknown);

                var typedNull = WithTypes(@null);
                typedNull.Type.ShouldEqual(NamedType.Nullable(new TypeVariable(6)));
            }
        }
    }
}