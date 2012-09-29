using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;
using Should;

namespace Rook.Compiling.Syntax
{
    [Facts]
    public class MethodInvocationTests : ExpressionTests
    {
        private readonly Class mathClass;

        public MethodInvocationTests()
        {
            mathClass = @"class Math
            {
                int Zero() 0;
                int Square(int x) x*x;
                bool Even(int n) if (n==0) true else Odd(n-1);
                bool Odd(int n) if (n==0) false else Even(n-1);
                int Max(int a, int b) if (a > b) a else b;
            }".ParseClass();
        }

        public void DemandsMethodNameWithOptionalArguments()
        {
            FailsToParse("instance.").AtEndOfInput().WithMessage("(1, 10): identifier expected");
            FailsToParse("instance.Method").AtEndOfInput().WithMessage("(1, 16): ( expected");
            FailsToParse("instance.Method(").AtEndOfInput().WithMessage("(1, 17): ) expected");

            Parses("instance.Method()").IntoTree("instance.Method()");
            Parses("instance.Method(1)").IntoTree("instance.Method(1)");
            Parses("instance.Method(1, 2, 3)").IntoTree("instance.Method(1, 2, 3)");

            Parses("0.Method()").IntoTree("0.Method()");
            Parses("true.Method(1)").IntoTree("true.Method(1)");
            Parses("[1, 2, 3].Method(4, 5, 6)").IntoTree("[1, 2, 3].Method(4, 5, 6)");

            Parses("instance.Method(1).Another(2, 3)").IntoTree("instance.Method(1).Another(2, 3)");

            FailsToParse("(instance.Method)()").LeavingUnparsedTokens(")", "(", ")").WithMessage("(1, 17): ( expected");
            FailsToParse("instance.(Method)()").LeavingUnparsedTokens("(", "Method", ")", "(", ")").WithMessage("(1, 10): identifier expected");
        }

        public void FailsTypeCheckingWhenInstanceExpressionFailsTypeChecking()
        {
            ShouldFailTypeChecking("math.Square(2)")
                .WithErrors(
                    error => error.ShouldEqual("Reference to undefined identifier: math", 1, 1),
                    error => error.ShouldEqual("Cannot invoke method against instance of unknown type.", 1, 5));
        }

        public void FailsTypeCheckingWhenInstanceExpressionTypeIsNotANamedType()
        {
            ShouldFailTypeChecking("math.Square(2)", math => new TypeVariable(123456)).WithError("Cannot invoke method against instance of unknown type.", 1, 5);
        }

        public void FailsTypeCheckingForUndefinedInstanceType()
        {
            ShouldFailTypeChecking("math.Square(2)", math => new NamedType(mathClass)).WithError("Type is undefined: Math", 1, 1);
        }
        public void TypeChecksAsExtensionMethodCallWhenPossibleForUndefinedInstanceType()
        {
            //NOTE: This test covers a temporary hack.  All types should be defined, even if that definition is empty.
            //      See note at the bottom of TypeChecker.TypeCheck(MethodInvocation methodInvocation, Scope scope).

            var node = (MethodInvocation)Parse("math.Square(2)");

            var typeChecker = new TypeChecker();

            var typedCall = (Call)typeChecker.TypeCheck(node, Scope(math => new NamedType(mathClass),
                                                                    Square => Function(new[] {new NamedType(mathClass), Integer}, Integer)));
            typedCall.Callable.Type.ShouldEqual(Function(new[] { new NamedType(mathClass), Integer }, Integer));
            typedCall.Arguments.ShouldHaveTypes(new NamedType(mathClass), Integer);
            typedCall.Type.ShouldEqual(Integer);
        }

        public void HasATypeEqualToTheReturnTypeOfTheMethod()
        {
            Type("math.Zero()", mathClass, math => new NamedType(mathClass)).ShouldEqual(Integer);
            Type("math.Even(1)", mathClass, math => new NamedType(mathClass)).ShouldEqual(Boolean);
            Type("math.Max(1, 2)", mathClass, math => new NamedType(mathClass)).ShouldEqual(Integer);
        }

        public void HasATypeEqualToTheInferredReturnTypeOfGenericMethods()
        {
            var utilityClass = "class Utility { }".ParseClass();
            var utilityType = new NamedType(utilityClass);

            var typeChecker = new TypeChecker();

            //Note: Rook methods cannot yet be declared as generic, so stub out a hypothetical method with signature: T Last<T>(Vector<T>)
            var x = new TypeVariable(123456);
            typeChecker.TypeMemberRegistry.Register(utilityType, new StubBinding("Last", Function(new[] { Vector.MakeGenericType(x) }, x)));

            Type("utility.Last([1, 2, 3])", typeChecker, utility => utilityType).ShouldEqual(Integer);
            Type("utility.Last([true, false])", typeChecker, utility => utilityType).ShouldEqual(Boolean);
        }

        public void TypeChecksArgumentExpressionsAgainstTheSurroundingScope()
        {
            Type("math.Max(zero, one)", mathClass, math => new NamedType(mathClass), zero => Integer, one => Integer).ShouldEqual(Integer);
        }

        public void DoesNotTypeCheckMethodNameAgainstTheSurroundingScope()
        {
            Type("math.Max(zero, one)", mathClass, math => new NamedType(mathClass), zero => Integer, one => Integer, Max => Boolean).ShouldEqual(Integer);
        }

        public void CanCreateFullyTypedInstance()
        {
            var node = (MethodInvocation)Parse("math.Even(two)");
            node.Instance.Type.ShouldEqual(Unknown);
            node.MethodName.Type.ShouldEqual(Unknown);
            node.Arguments.Single().Type.ShouldEqual(Unknown);
            node.Type.ShouldEqual(Unknown);

            var typedNode = WithTypes(node, mathClass, math => new NamedType(mathClass), two => Integer);
            typedNode.Instance.Type.ShouldEqual(new NamedType(mathClass));
            typedNode.MethodName.Type.ShouldEqual(NamedType.Function(new[] { Integer }, Boolean));
            typedNode.Arguments.Single().Type.ShouldEqual(Integer);
            typedNode.Type.ShouldEqual(Boolean);
        }
        public void TypeChecksAsExtensionMethodCallWhenInstanceTypeDoesNotContainMethodWithExpectedName()
        {
            var typeChecker = new TypeChecker();
            typeChecker.TypeMemberRegistry.Register(mathClass);

            var node = (MethodInvocation)Parse("math.SomeExtensionMethod(false)");

            var typedCall = (Call)typeChecker.TypeCheck(node, Scope(math => new NamedType(mathClass),
                                                                    SomeExtensionMethod => Function(new[] { new NamedType(mathClass), Boolean }, Integer)));

            typedCall.Callable.Type.ShouldEqual(Function(new[] { new NamedType(mathClass), Boolean }, Integer));
            typedCall.Arguments.ShouldHaveTypes(new NamedType(mathClass), Boolean);
            typedCall.Type.ShouldEqual(Integer);
        }

        public void FailsTypeCheckingForIncorrectNumberOfArguments()
        {
            ShouldFailTypeChecking("math.Zero(false)", mathClass, math => new NamedType(mathClass)).WithError(
                "Type mismatch: expected System.Func<int>, found System.Func<bool, int>.", 1, 5);

            ShouldFailTypeChecking("math.Square(1, true)", mathClass, math => new NamedType(mathClass)).WithError(
                "Type mismatch: expected System.Func<int, int>, found System.Func<int, bool, int>.", 1, 5);

            ShouldFailTypeChecking("math.Max(1, 2, 3)", mathClass, math => new NamedType(mathClass)).WithError(
                "Type mismatch: expected System.Func<int, int, int>, found System.Func<int, int, int, int>.", 1, 5);
        }

        public void FailsTypeCheckingForMismatchedArgumentTypes()
        {
            ShouldFailTypeChecking("math.Square(true)", mathClass, math => new NamedType(mathClass)).WithError(
                "Type mismatch: expected int, found bool.", 1, 5);
        }

        public void FailsTypeCheckingWhenAttemptingToCallANoncallableMember()
        {
            var sampleClass = "class Sample { }".ParseClass();
            var sampleType = new NamedType(sampleClass);

            var typeChecker = new TypeChecker();

            //Note: Rook classes cannot yet contain fields/properties, so stub out a hypothetical property member binding.
            typeChecker.TypeMemberRegistry.Register(sampleType, new StubBinding("IntegerProperty", Integer));

            ShouldFailTypeChecking("sample.IntegerProperty()", typeChecker, sample => sampleType).WithError("Attempted to call a noncallable object.", 1, 7);
        }

        private DataType Type(string source, Class knownClass, params TypeMapping[] symbols)
        {
            var typeChecker = new TypeChecker();
            typeChecker.TypeMemberRegistry.Register(knownClass);

            return Type(source, typeChecker, symbols);
        }

        private Vector<CompilerError> ShouldFailTypeChecking(string source, Class knownClass, params TypeMapping[] symbols)
        {
            var typeChecker = new TypeChecker();
            typeChecker.TypeMemberRegistry.Register(knownClass);

            return ShouldFailTypeChecking(source, typeChecker, symbols);
        }

        private T WithTypes<T>(T syntaxTree, Class knownClass, params TypeMapping[] symbols) where T : Expression
        {
            var typeChecker = new TypeChecker();
            typeChecker.TypeMemberRegistry.Register(knownClass);

            return WithTypes(syntaxTree, typeChecker, symbols);
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