using Rook.Compiling.Types;
using Should;

namespace Rook.Compiling.Syntax
{
    [Facts]
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
                Type("null").ShouldEqual(Nullable.MakeGenericType(new TypeVariable(2)));
            }
        }

        public void CanCreateFullyTypedInstance()
        {
            using (TypeVariable.TestFactory())
            {
                var @null = (Null) Parse("null");
                @null.Type.ShouldEqual(Unknown);

                var typedNull = WithTypes(@null);
                typedNull.Type.ShouldEqual(Nullable.MakeGenericType(new TypeVariable(2)));
            }
        }
    }
}