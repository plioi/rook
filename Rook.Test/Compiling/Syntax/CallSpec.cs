using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class CallSpec : ExpressionSpec
    {
        [Test]
        public void FunctionCalls()
        {
            AssertTree("(func())", "func()");
            AssertTree("(func(2))", "func(2)");
            AssertTree("(func(1, 2, 3))", "func(1, 2, 3)");
            AssertTree("(func())", "(func)()");
            AssertTree("(func(1))", "(func)(1)");
            AssertTree("(func(1, 2, 3))", "(func)(1, 2, 3)");
            AssertTree("((getFunc(true, false))(1))", "(getFunc(true, false))(1)");
            AssertTree("({func;}(1, 2))", "{func;}(1, 2)");
        }

        [Test]
        public void TreatsIndexOperatorsAsCallToIndexFunction()
        {
            AssertTree("(Index(vector, 0))", "vector[0]");
            AssertTree("(Index(vector, 0))", "(vector)[0]");
            AssertTree("(Index((getVector(true, false)), 1))", "(getVector(true, false))[1]");
            AssertTree("(Index((getVector(true, false)), ((1) + (2))))", "(getVector(true, false))[1+2]");
            AssertTree("(Index([0], 0))", "[0][0]");
            AssertTree("(Index([0, 1], 1))", "[0, 1][1]");
            AssertTree("(Index({[0, 1];}, 1))", "{[0, 1];}[1]");
        }

        [Test]
        public void TreatsSliceOperatorsAsCallToSliceFunction()
        {
            AssertTree("(Slice(vector, 0, 5))", "vector[0:5]");
            AssertTree("(Slice(vector, 0, 5))", "(vector)[0:5]");
            AssertTree("(Slice((getVector(true, false)), 1, 6))", "(getVector(true, false))[1:6]");
            AssertTree("(Slice((getVector(true, false)), ((1) + (2)), ((3) + (4))))", "(getVector(true, false))[1+2:3+4]");
            AssertTree("(Slice([0], 0, 0))", "[0][0:0]");
            AssertTree("(Slice([0, 1], 0, 1))", "[0, 1][0:1]");
            AssertTree("(Slice({[0, 1];}, 0, 1))", "{[0, 1];}[0:1]");
        }

        [Test]
        public void UnaryOperations()
        {
            AssertTree("(-(1))", "-1");
            AssertTree("(!(false))", "!false");
        }

        [Test]
        public void MultiplicativeOperations()
        {
            AssertTree("((1) * (2))", "1*2");
            AssertTree("((1) / (2))", "1/2");
        }

        [Test]
        public void AdditiveOperations()
        {
            AssertTree("((1) + (2))", "1+2");
            AssertTree("((1) - (2))", "1-2");
        }

        [Test]
        public void RelationalOperations()
        {
            AssertTree("((1) < (2))", "1<2");
            AssertTree("((1) > (2))", "1>2");
            AssertTree("((1) <= (2))", "1<=2");
            AssertTree("((1) >= (2))", "1>=2");
        }

        [Test]
        public void EqualityOperations()
        {
            AssertTree("((1) == (2))", "1==2");
            AssertTree("((1) != (2))", "1!=2");
        }

        [Test]
        public void LogicalAndOperations()
        {
            AssertTree("((true) && (false))", "true&&false");
        }

        [Test]
        public void LogicalOrOperations()
        {
            AssertTree("((true) || (false))", "true||false");
        }

        [Test]
        public void NullCoalescingOperations()
        {
            AssertTree("((null) ?? (0))", "null??0");
        }

        [Test]
        public void TreatsInfixOperationsAsLeftAssociative()
        {
            AssertTree("((((1) * (2))) * (3))", "1*2*3");
            AssertTree("((((1) + (2))) + (3))", "1+2+3");
            AssertTree("((((true) && (false))) && (false))", "true&&false&&false");
            AssertTree("((((false) || (true))) || (false))", "false||true||false");
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
            Call node = (Call)Parse("func(yes, zero)");
            Assert.IsNull(node.Callable.Type);
            Assert.IsNull(node.Arguments.ElementAt(0).Type);
            Assert.IsNull(node.Arguments.ElementAt(1).Type);
            Assert.IsNull(node.Type);

            Call typedNode = (Call)node.WithTypes(Environment(func => Function(new[] {Boolean, Integer}, Integer), yes => Boolean, zero => Integer)).Syntax;
            Assert.AreSame(NamedType.Function(new[] {Boolean, Integer}, Integer), typedNode.Callable.Type);
            Assert.AreSame(Boolean, typedNode.Arguments.ElementAt(0).Type);
            Assert.AreSame(Integer, typedNode.Arguments.ElementAt(1).Type);
            Assert.AreSame(Integer, typedNode.Type);
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