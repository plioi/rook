using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public static class TypeCheckingAssertions
    {
        public static void ShouldHaveTypes(this IEnumerable<TypedSyntaxTree> actual, params DataType[] expectedTypes)
        {
            actual.Select(x => x.Type).ShouldList(expectedTypes);
        }

        public static void WithError(this Vector<CompilerError> actualErrors, string expectedMessage, int expectedLine, int expectedColumn)
        {
            var expectedPosition = new Position(expectedLine, expectedColumn);

            if (actualErrors.Count() != 1)
            {
                Fail.WithErrors(actualErrors, expectedPosition, expectedMessage);
            }
            else
            {
                var error = actualErrors.First();

                if (expectedPosition != error.Position || expectedMessage != error.Message)
                    Fail.WithErrors(actualErrors, expectedPosition, expectedMessage);
            }
        }
    }
}
