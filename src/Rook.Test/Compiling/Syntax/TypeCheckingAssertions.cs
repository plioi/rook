using System;
using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;
using Should;

namespace Rook.Compiling.Syntax
{
    public static class TypeCheckingAssertions
    {
        public static void ShouldEqual(this NamedType actual, string expectedName, string expectedToString, params DataType[] expectedGenericArguments)
        {
            actual.Name.ShouldEqual(expectedName);
            actual.ToString().ShouldEqual(expectedToString);

            if (expectedGenericArguments.Any())
                actual.GenericArguments.ShouldList(expectedGenericArguments);
            else
                actual.IsGeneric.ShouldBeFalse();
        }

        public static void ShouldHaveTypes(this IEnumerable<TypedSyntaxTree> actual, params DataType[] expectedTypes)
        {
            actual.Select(x => x.Type).ShouldList(expectedTypes);
        }

        public static void ShouldEqual(this CompilerError actual, string expectedMessage, int expectedLine, int expectedColumn)
        {
            var expectedPosition = new Position(expectedLine, expectedColumn);

            actual.Position.ShouldEqual(expectedPosition);
            actual.Message.ShouldEqual(expectedMessage);
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

        public static void WithErrors<T>(this IEnumerable<T> actualErrors, params Action<T>[] errorExpectations)
        {
            actualErrors.ShouldList(errorExpectations);
        }
    }
}
