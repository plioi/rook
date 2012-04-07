using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public static class TypeCheckingExtensions
    {
        public static void ShouldHaveTypes(this IEnumerable<TypedSyntaxTree> actual, params DataType[] expectedTypes)
        {
            actual.Select(x => x.Type).ShouldList(expectedTypes);
        }
    }
}
