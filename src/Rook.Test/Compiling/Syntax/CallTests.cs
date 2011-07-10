using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public class CallTests : ExpressionTests
    {
        [Test]
        public void FunctionCalls()
        {
            Parses("func()").IntoTree("(func())");
            Parses("func(2)").IntoTree("(func(2))");
            Parses("func(1, 2, 3)").IntoTree("(func(1, 2, 3))");
            Parses("(func)()").IntoTree("(func())");
            Parses("(func)(1)").IntoTree("(func(1))");
            Parses("(func)(1, 2, 3)").IntoTree("(func(1, 2, 3))");
            Parses("(getFunc(true, false))(1)").IntoTree("((getFunc(true, false))(1))");
            Parses("{func;}(1, 2)").IntoTree("({func;}(1, 2))");
        }

        [Test]
        public void TreatsIndexOperatorsAsCallToIndexFunction()
        {
            Parses("vector[0]").IntoTree("(Index(vector, 0))");
            Parses("(vector)[0]").IntoTree("(Index(vector, 0))");
            Parses("(getVector(true, false))[1]").IntoTree("(Index((getVector(true, false)), 1))");
            Parses("(getVector(true, false))[1+2]").IntoTree("(Index((getVector(true, false)), ((1) + (2))))");
            Parses("[0][0]").IntoTree("(Index([0], 0))");
            Parses("[0, 1][1]").IntoTree("(Index([0, 1], 1))");
            Parses("{[0, 1];}[1]").IntoTree("(Index({[0, 1];}, 1))");
        }

        [Test]
        public void TreatsSliceOperatorsAsCallToSliceFunction()
        {
            Parses("vector[0:5]").IntoTree("(Slice(vector, 0, 5))");
            Parses("(vector)[0:5]").IntoTree("(Slice(vector, 0, 5))");
            Parses("(getVector(true, false))[1:6]").IntoTree("(Slice((getVector(true, false)), 1, 6))");
            Parses("(getVector(true, false))[1+2:3+4]").IntoTree("(Slice((getVector(true, false)), ((1) + (2)), ((3) + (4))))");
            Parses("[0][0:0]").IntoTree("(Slice([0], 0, 0))");
            Parses("[0, 1][0:1]").IntoTree("(Slice([0, 1], 0, 1))");
            Parses("{[0, 1];}[0:1]").IntoTree("(Slice({[0, 1];}, 0, 1))");
        }

        [Test]
        public void UnaryOperations()
        {
            Parses("-1").IntoTree("(-(1))");
            Parses("!false").IntoTree("(!(false))");
        }

        [Test]
        public void MultiplicativeOperations()
        {
            Parses("1*2").IntoTree("((1) * (2))");
            Parses("1/2").IntoTree("((1) / (2))");
        }

        [Test]
        public void AdditiveOperations()
        {
            Parses("1+2").IntoTree("((1) + (2))");
            Parses("1-2").IntoTree("((1) - (2))");
        }

        [Test]
        public void RelationalOperations()
        {
            Parses("1<2").IntoTree("((1) < (2))");
            Parses("1>2").IntoTree("((1) > (2))");
            Parses("1<=2").IntoTree("((1) <= (2))");
            Parses("1>=2").IntoTree("((1) >= (2))");
        }

        [Test]
        public void EqualityOperations()
        {
            Parses("1==2").IntoTree("((1) == (2))");
            Parses("1!=2").IntoTree("((1) != (2))");
        }

        [Test]
        public void LogicalAndOperations()
        {
            Parses("true&&false").IntoTree("((true) && (false))");
        }

        [Test]
        public void LogicalOrOperations()
        {
            Parses("true||false").IntoTree("((true) || (false))");
        }

        [Test]
        public void NullCoalescingOperations()
        {
            Parses("null??0").IntoTree("((null) ?? (0))");
        }

        [Test]
        public void TreatsInfixOperationsAsLeftAssociative()
        {
            Parses("1*2*3").IntoTree("((((1) * (2))) * (3))");
            Parses("1+2+3").IntoTree("((((1) + (2))) + (3))");
            Parses("true&&false&&false").IntoTree("((((true) && (false))) && (false))");
            Parses("false||true||false").IntoTree("((((false) || (true))) || (false))");
        }

        [Test]
        public void HasATypeEqualToTheReturnTypeOfTheCallableObject()
        {
            AssertType(Integer, "func()", func => Function(Integer));
            AssertType(Boolean, "func(1)", func => Function(new[] { Integer }, Boolean));
            AssertType(Integer, "func(false, 1)", func => Function(new[] { Boolean, Integer }, Integer));
        }

        [Test]
        public void HasATypeEqualToTheInferredReturnTypeOfGenericCallableObjects()
        {
            var x = new TypeVariable(123456);

            AssertType(Integer, "func([1, 2, 3])", func => Function(new[] {Vector(x)}, x));
            AssertType(Boolean, "func([true, false])", func => Function(new[] {Vector(x)}, x));
        }

        [Test]
        public void TypeChecksArgumentExpressionsAgainstTheSurroundingScope()
        {
            AssertType(Integer, "func(yes, zero)", func => Function(new[] {Boolean, Integer}, Integer), yes => Boolean, zero => Integer);
        }

        [Test]
        public void CanCreateFullyTypedInstance()
        {
            var node = (Call)Parse("func(yes, zero)");
            node.Callable.Type.ShouldBeNull();
            node.Arguments.ElementAt(0).Type.ShouldBeNull();
            node.Arguments.ElementAt(1).Type.ShouldBeNull();
            node.Type.ShouldBeNull();

            var typedNode = (Call)node.WithTypes(Environment(func => Function(new[] {Boolean, Integer}, Integer), yes => Boolean, zero => Integer)).Syntax;
            typedNode.Callable.Type.ShouldEqual(NamedType.Function(new[] { Boolean, Integer }, Integer));
            typedNode.Arguments.ElementAt(0).Type.ShouldEqual(Boolean);
            typedNode.Arguments.ElementAt(1).Type.ShouldEqual(Integer);
            typedNode.Type.ShouldEqual(Integer);
        }

        [Test]
        public void FailsTypeCheckingForIncorrectNumberOfArguments()
        {
            NamedType twoArgsToInteger =
                NamedType.Function(new[] {NamedType.Boolean, NamedType.Integer}, NamedType.Integer);

            AssertTypeCheckError(
                1, 1,
                "Type mismatch: expected System.Func<bool, int, int>, found System.Func<bool, int, bool, int>.",
                "foo(true, 1, false)", foo => twoArgsToInteger);
        }

        [Test]
        public void FailsTypeCheckingForMismatchedArgumentTypes()
        {
            NamedType integerToBoolean =
                NamedType.Function(new[] {NamedType.Integer}, NamedType.Boolean);

            AssertTypeCheckError(
                1, 1,
                "Type mismatch: expected int, found bool.",
                "even(true)", even => integerToBoolean);
        }

        [Test]
        public void FailsTypeCheckingWhenAttemptingToCallANoncallableObject()
        {
            AssertTypeCheckError(
                1, 1,
                "Attempted to call a noncallable object.",
                "zero()", zero => Integer);
        }

        private static DataType Function(IEnumerable<DataType> parameterTypes, DataType returnType)
        {
            return NamedType.Function(parameterTypes, returnType);
        }

        private static DataType Function(DataType returnType)
        {
            return Function(new DataType[] { }, returnType);
        }

        private static NamedType Vector(DataType itemType)
        {
            return NamedType.Vector(itemType);
        }
    }
}