using System;
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
        private readonly Class knownClass;

        public MethodInvocationTests()
        {
            knownClass = ParseClass(@"class Math
            {
                int Zero() 0;
                int Square(int x) x*x;
                bool Even(int n) if (n==0) true else Odd(n-1);
                bool Odd(int n) if (n==0) false else Even(n-1);
                int Max(int a, int b) if (a > b) a else b;
            }");
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
            ShouldFailTypeChecking("math.Square(2)", math => new NamedType("Math")).WithError("Type is undefined: Math", 1, 1);
        }
        public void TypeChecksAsExtensionMethodCallWhenPossibleForUndefinedInstanceType()
        {
            //NOTE: This test covers a temporary hack.  All types should be defined, even if that definition is empty.
            //      See note at the bottom of TypeChecker.TypeCheck(MethodInvocation methodInvocation, Scope scope).

            var node = (MethodInvocation)Parse("math.Square(2)");

            var typeChecker = new TypeChecker();

            var typedCall = (Call)typeChecker.TypeCheck(node, Scope(math => new NamedType("Math"),
                                                                    Square => Function(new[] {new NamedType("Math"), Integer}, Integer)));
            typedCall.Callable.Type.ShouldEqual(Function(new[] { new NamedType("Math"), Integer }, Integer));
            typedCall.Arguments.ShouldHaveTypes(new NamedType("Math"), Integer);
            typedCall.Type.ShouldEqual(Integer);
        }

        public void HasATypeEqualToTheReturnTypeOfTheMethod()
        {
            Type("math.Zero()", knownClass, math => new NamedType("Math")).ShouldEqual(Integer);
            Type("math.Even(1)", knownClass, math => new NamedType("Math")).ShouldEqual(Boolean);
            Type("math.Max(1, 2)", knownClass, math => new NamedType("Math")).ShouldEqual(Integer);
        }

        public void HasATypeEqualToTheInferredReturnTypeOfGenericMethods()
        {
            var x = new TypeVariable(123456);

            var typeChecker = new TypeChecker();
            typeChecker.TypeRegistry.Register(new NamedType("Utility"), new StubBinding("Last", Function(new[] { Vector.MakeGenericType(x) }, x)));

            Type("utility.Last([1, 2, 3])", typeChecker, utility => new NamedType("Utility")).ShouldEqual(Integer);
            Type("utility.Last([true, false])", typeChecker, utility => new NamedType("Utility")).ShouldEqual(Boolean);
        }

        public void TypeChecksArgumentExpressionsAgainstTheSurroundingScope()
        {
            Type("math.Max(zero, one)", knownClass, math => new NamedType("Math"), zero => Integer, one => Integer).ShouldEqual(Integer);
        }

        public void DoesNotTypeCheckMethodNameAgainstTheSurroundingScope()
        {
            Type("math.Max(zero, one)", knownClass, math => new NamedType("Math"), zero => Integer, one => Integer, Max => Boolean).ShouldEqual(Integer);
        }

        public void CanCreateFullyTypedInstance()
        {
            var node = (MethodInvocation)Parse("math.Even(two)");
            node.Instance.Type.ShouldEqual(Unknown);
            node.MethodName.Type.ShouldEqual(Unknown);
            node.Arguments.Single().Type.ShouldEqual(Unknown);
            node.Type.ShouldEqual(Unknown);

            var typedNode = WithTypes(node, knownClass, math => new NamedType("Math"), two => Integer);
            typedNode.Instance.Type.ShouldEqual(new NamedType("Math"));
            typedNode.MethodName.Type.ShouldEqual(NamedType.Function(new[] { Integer }, Boolean));
            typedNode.Arguments.Single().Type.ShouldEqual(Integer);
            typedNode.Type.ShouldEqual(Boolean);
        }
        public void TypeChecksAsExtensionMethodCallWhenInstanceTypeDoesNotContainMethodWithExpectedName()
        {
            var typeChecker = new TypeChecker();
            typeChecker.TypeRegistry.Register(knownClass);

            var node = (MethodInvocation)Parse("math.SomeExtensionMethod(false)");

            var typedCall = (Call)typeChecker.TypeCheck(node, Scope(math => new NamedType("Math"),
                                                                    SomeExtensionMethod => Function(new[] { new NamedType("Math"), Boolean }, Integer)));

            typedCall.Callable.Type.ShouldEqual(Function(new[] { new NamedType("Math"), Boolean }, Integer));
            typedCall.Arguments.ShouldHaveTypes(new NamedType("Math"), Boolean);
            typedCall.Type.ShouldEqual(Integer);
        }

        public void FailsTypeCheckingForIncorrectNumberOfArguments()
        {
            ShouldFailTypeChecking("math.Zero(false)", knownClass, math => new NamedType("Math")).WithError(
                "Type mismatch: expected System.Func<int>, found System.Func<bool, int>.", 1, 5);

            ShouldFailTypeChecking("math.Square(1, true)", knownClass, math => new NamedType("Math")).WithError(
                "Type mismatch: expected System.Func<int, int>, found System.Func<int, bool, int>.", 1, 5);

            ShouldFailTypeChecking("math.Max(1, 2, 3)", knownClass, math => new NamedType("Math")).WithError(
                "Type mismatch: expected System.Func<int, int, int>, found System.Func<int, int, int, int>.", 1, 5);
        }

        public void FailsTypeCheckingForMismatchedArgumentTypes()
        {
            ShouldFailTypeChecking("math.Square(true)", knownClass, math => new NamedType("Math")).WithError(
                "Type mismatch: expected int, found bool.", 1, 5);
        }

        public void FailsTypeCheckingWhenAttemptingToCallANoncallableMember()
        {
            var typeChecker = new TypeChecker();
            typeChecker.TypeRegistry.Register(new NamedType("Sample"), new StubBinding("IntegerProperty", Integer));

            ShouldFailTypeChecking("sample.IntegerProperty()", typeChecker, sample => new NamedType("Sample")).WithError("Attempted to call a noncallable object.", 1, 7);
        }

        private static Class ParseClass(string classDeclaration)
        {
            var tokens = new RookLexer().Tokenize(classDeclaration);
            var parser = new RookGrammar().Class;
            return parser.Parse(new TokenStream(tokens)).Value;
        }

        private DataType Type(string source, Class knownClass, params TypeMapping[] symbols)
        {
            var typeChecker = new TypeChecker();
            typeChecker.TypeRegistry.Register(knownClass);

            return Type(source, typeChecker, symbols);
        }

        private Vector<CompilerError> ShouldFailTypeChecking(string source, Class knownClass, params TypeMapping[] symbols)
        {
            var typeChecker = new TypeChecker();
            typeChecker.TypeRegistry.Register(knownClass);

            return ShouldFailTypeChecking(source, typeChecker, symbols);
        }

        private T WithTypes<T>(T syntaxTree, Class knownClass, params TypeMapping[] symbols) where T : Expression
        {
            var typeChecker = new TypeChecker();
            typeChecker.TypeRegistry.Register(knownClass);

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

        private class StubBinding : Binding
        {
            public StubBinding(string identifier, DataType type)
            {
                Type = type;
                Identifier = identifier;
            }

            public Position Position
            {
                get { throw new NotImplementedException(); }
            }

            public string Identifier { get; private set; }

            public DataType Type { get; private set; }
        }
    }
}