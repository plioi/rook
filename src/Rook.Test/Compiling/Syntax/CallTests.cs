using System.Collections.Generic;
using Rook.Compiling.Types;
using Should;

namespace Rook.Compiling.Syntax
{
    [Facts]
    public class CallTests : ExpressionTests
    {
        public void FunctionCalls()
        {
            Parses("func()").IntoTree("(func())");
            Parses("func(2)").IntoTree("(func(2))");
            Parses("func(1, 2, 3)").IntoTree("(func(1, 2, 3))");
            Parses("(func)()").IntoTree("(func())");
            Parses("(func)(1)").IntoTree("(func(1))");
            Parses("(func)(1, 2, 3)").IntoTree("(func(1, 2, 3))");
            Parses("(getFunc(true, false))(1)").IntoTree("((getFunc(true, false))(1))");
            Parses("{func}(1, 2)").IntoTree("({func}(1, 2))");
        }

        public void TreatsIndexOperatorsAsCallToIndexMethod()
        {
            Parses("vector[0]").IntoTree("vector.__index__(0)");
            Parses("(vector)[0]").IntoTree("vector.__index__(0)");
            Parses("(getVector(true, false))[1]").IntoTree("(getVector(true, false)).__index__(1)");
            Parses("(getVector(true, false))[1+2]").IntoTree("(getVector(true, false)).__index__(((1) + (2)))");
            Parses("[0][0]").IntoTree("[0].__index__(0)");
            Parses("[0, 1][1]").IntoTree("[0, 1].__index__(1)");
            Parses("{[0, 1]}[1]").IntoTree("{[0, 1]}.__index__(1)");
        }

        public void TreatsSliceOperatorsAsCallToSliceFunction()
        {
            Parses("vector[0:5]").IntoTree("vector.__slice__(0, 5)");
            Parses("(vector)[0:5]").IntoTree("vector.__slice__(0, 5)");
            Parses("(getVector(true, false))[1:6]").IntoTree("(getVector(true, false)).__slice__(1, 6)");
            Parses("(getVector(true, false))[1+2:3+4]").IntoTree("(getVector(true, false)).__slice__(((1) + (2)), ((3) + (4)))");
            Parses("[0][0:0]").IntoTree("[0].__slice__(0, 0)");
            Parses("[0, 1][0:1]").IntoTree("[0, 1].__slice__(0, 1)");
            Parses("{[0, 1]}[0:1]").IntoTree("{[0, 1]}.__slice__(0, 1)");
        }

        public void UnaryOperations()
        {
            Parses("-1").IntoTree("(-(1))");
            Parses("!false").IntoTree("(!(false))");
        }

        public void MultiplicativeOperations()
        {
            Parses("1*2").IntoTree("((1) * (2))");
            Parses("1/2").IntoTree("((1) / (2))");
        }

        public void AdditiveOperations()
        {
            Parses("1+2").IntoTree("((1) + (2))");
            Parses("1-2").IntoTree("((1) - (2))");
        }

        public void RelationalOperations()
        {
            Parses("1<2").IntoTree("((1) < (2))");
            Parses("1>2").IntoTree("((1) > (2))");
            Parses("1<=2").IntoTree("((1) <= (2))");
            Parses("1>=2").IntoTree("((1) >= (2))");
        }

        public void EqualityOperations()
        {
            Parses("1==2").IntoTree("((1) == (2))");
            Parses("1!=2").IntoTree("((1) != (2))");
        }

        public void LogicalAndOperations()
        {
            Parses("true&&false").IntoTree("((true) && (false))");
        }

        public void LogicalOrOperations()
        {
            Parses("true||false").IntoTree("((true) || (false))");
        }

        public void NullCoalescingOperations()
        {
            Parses("null??0").IntoTree("((null) ?? (0))");
        }

        public void TreatsBinaryOperationsAsLeftAssociative()
        {
            Parses("1*2*3").IntoTree("((((1) * (2))) * (3))");
            Parses("1/2/3").IntoTree("((((1) / (2))) / (3))");
            Parses("1+2+3").IntoTree("((((1) + (2))) + (3))");
            Parses("1-2-3").IntoTree("((((1) - (2))) - (3))");
            Parses("1<2<3").IntoTree("((((1) < (2))) < (3))");
            Parses("1>2>3").IntoTree("((((1) > (2))) > (3))");
            Parses("1<=2<=3").IntoTree("((((1) <= (2))) <= (3))");
            Parses("1>=2>=3").IntoTree("((((1) >= (2))) >= (3))");
            Parses("1==2==3").IntoTree("((((1) == (2))) == (3))");
            Parses("1!=2!=3").IntoTree("((((1) != (2))) != (3))");
            Parses("true&&false&&false").IntoTree("((((true) && (false))) && (false))");
            Parses("false||true||false").IntoTree("((((false) || (true))) || (false))");
            Parses("x??y??z").IntoTree("((((x) ?? (y))) ?? (z))");
        }

        public void HasATypeEqualToTheReturnTypeOfTheCallableObject()
        {
            Type("func()", func => Function(Integer)).ShouldEqual(Integer);
            Type("func(1)", func => Function(new[] { Integer }, Boolean)).ShouldEqual(Boolean);
            Type("func(false, 1)", func => Function(new[] { Boolean, Integer }, Integer)).ShouldEqual(Integer);
        }

        public void HasATypeEqualToTheInferredReturnTypeOfGenericCallableObjects()
        {
            var x = new TypeVariable(123456);

            Type("func([1, 2, 3])", func => Function(new[] { NamedType.Vector(x) }, x)).ShouldEqual(Integer);
            Type("func([true, false])", func => Function(new[] { NamedType.Vector(x) }, x)).ShouldEqual(Boolean);
        }

        public void TypeChecksArgumentExpressionsAgainstTheSurroundingScope()
        {
            Type("func(yes, zero)", func => Function(new[] { Boolean, Integer }, Integer), yes => Boolean, zero => Integer).ShouldEqual(Integer);
        }

        public void CanCreateFullyTypedInstance()
        {
            var call = (Call)Parse("func(yes, zero)");
            call.Callable.Type.ShouldEqual(Unknown);
            call.Arguments.ShouldHaveTypes(Unknown, Unknown);
            call.Type.ShouldEqual(Unknown);

            var typedCall = WithTypes(call,
                                      func => Function(new[] {Boolean, Integer}, Integer),
                                      yes => Boolean,
                                      zero => Integer);
            typedCall.Callable.Type.ShouldEqual(NamedType.Function(new[] { Boolean, Integer }, Integer));
            typedCall.Arguments.ShouldHaveTypes(Boolean, Integer);
            typedCall.Type.ShouldEqual(Integer);
        }

        public void FailsTypeCheckingForIncorrectNumberOfArguments()
        {
            var twoArgsToInteger = NamedType.Function(new[] {NamedType.Boolean, NamedType.Integer}, NamedType.Integer);

            ShouldFailTypeChecking("foo(true, 1, false)", foo => twoArgsToInteger).WithError(
                "Type mismatch: expected System.Func<bool, int, int>, found System.Func<bool, int, bool, int>.", 1, 1);
        }

        public void FailsTypeCheckingForMismatchedArgumentTypes()
        {
            var integerToBoolean = NamedType.Function(new[] {NamedType.Integer}, NamedType.Boolean);

            ShouldFailTypeChecking("even(true)", even => integerToBoolean).WithError(
                "Type mismatch: expected int, found bool.", 1, 1);
        }

        public void FailsTypeCheckingWhenAttemptingToCallANoncallableObject()
        {
            ShouldFailTypeChecking("zero()", zero => Integer).WithError("Attempted to call a noncallable object.", 1, 1);
        }

        private static DataType Function(IEnumerable<DataType> parameterTypes, DataType returnType)
        {
            return NamedType.Function(parameterTypes, returnType);
        }

        private static DataType Function(DataType returnType)
        {
            return Function(new DataType[] { }, returnType);
        }
    }
}