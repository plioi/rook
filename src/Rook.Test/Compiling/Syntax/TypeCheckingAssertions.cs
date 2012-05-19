using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Should;

namespace Rook.Compiling.Syntax
{
    public static class TypeCheckingAssertions
    {
        public static void ShouldHaveTypes(this IEnumerable<TypedSyntaxTree> actual, params DataType[] expectedTypes)
        {
            actual.Select(x => x.Type).ShouldList(expectedTypes);
        }

        public static void ShouldFail<TSyntax>(this TypeChecked<TSyntax> typeChecked, string expectedMessage, int expectedLine, int expectedColumn) where TSyntax : SyntaxTree
        {
            var expectedPosition = new Position(expectedLine, expectedColumn);

            //TODO: If !typeChecked.HasErrors, fail with a description
            //      of the unexpectedly successful type checking.

            typeChecked.Syntax.ShouldBeNull();
            typeChecked.HasErrors.ShouldBeTrue();

            if (typeChecked.Errors.Count() != 1)
            {
                Fail.WithErrors(typeChecked.Errors, expectedPosition, expectedMessage);
            }
            else
            {
                var error = typeChecked.Errors.First();

                if (expectedPosition != error.Position || expectedMessage != error.Message)
                    Fail.WithErrors(typeChecked.Errors, expectedPosition, expectedMessage);
            }
        }
    }
}
