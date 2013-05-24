using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;
using Should;

namespace Rook.Compiling.Syntax
{
    public class ClassTests : SyntaxTreeTests<Class>
    {
        protected override Parser<Class> Parser { get { return RookGrammar.Class; } }

        public void ParsesEmptyClassDeclarations()
        {
            FailsToParse("").AtEndOfInput().WithMessage("(1, 1): class expected");
            FailsToParse("class").AtEndOfInput().WithMessage("(1, 6): identifier expected");
            FailsToParse("class Foo").AtEndOfInput().WithMessage("(1, 10): { expected");
            FailsToParse("class Foo {").AtEndOfInput().WithMessage("(1, 12): } expected");
            Parses("class Foo {}").IntoTree("class Foo {}");
        }

        public void ParsesMethods()
        {
            Parses("class Hitchhiker {int life() {42} int universe() {42} int everything() {42}}")
                .IntoTree("class Hitchhiker {int life() {42} int universe() {42} int everything() {42}}");
        }

        public void DemandsEndOfClassAfterLastValidMethod()
        {
            FailsToParse("class Hitchhiker { int life() {42}").AtEndOfInput().WithMessage("(1, 35): } expected");
        }

        public void HasATypeCorrespondingWithTheDefaultConstructor()
        {
            var fooClass = "class Foo { }".ParseClass();

            var typeRegistry = new TypeRegistry();
            var constructorReturningFoo = NamedType.Constructor(new NamedType(fooClass));
            typeRegistry.Add(fooClass);

            var typeChecker = new TypeChecker(typeRegistry);
            typeChecker.TypeCheck(fooClass, Scope()).Type.ShouldEqual(constructorReturningFoo);
            typeChecker.HasErrors.ShouldBeFalse();
        }

        public void PassesTypeCheckingEvenWhenMethodNamesAreTheSameAsNamesInTheSurroundingScope()
        {
            var fooClass = "class Foo { int A() {0} int B() {2} }".ParseClass();

            var typeRegistry = new TypeRegistry();
            var constructorReturningFoo = NamedType.Constructor(new NamedType(fooClass));
            typeRegistry.Add(fooClass);

            var typeChecker = new TypeChecker(typeRegistry);
            typeChecker.TypeCheck(fooClass, Scope(B => NamedType.Function(NamedType.Boolean))).Type.ShouldEqual(constructorReturningFoo);
            typeChecker.HasErrors.ShouldBeFalse();
        }

        public void CanCreateFullyTypedInstance()
        {
            var @class = @"class Foo
                           {
                              bool Even(int n) { if (n==0) true else Odd(n-1) }
                              bool Odd(int n) { if (n==0) false else Even(n-1) }
                              int Test() { if (Even(4)) 0 else 1 }
                           }".ParseClass();

            var constructorReturningFoo = NamedType.Constructor(new NamedType(@class));

            @class.Methods.ShouldList(
                even =>
                {
                    even.Name.Identifier.ShouldEqual("Even");
                    even.Type.ShouldEqual(Unknown);
                    even.Body.Type.ShouldEqual(Unknown);
                },
                odd =>
                {
                    odd.Name.Identifier.ShouldEqual("Odd");
                    odd.Type.ShouldEqual(Unknown);
                    odd.Body.Type.ShouldEqual(Unknown);
                },
                test =>
                {
                    test.Name.Identifier.ShouldEqual("Test");
                    test.Type.ShouldEqual(Unknown);
                    test.Body.Type.ShouldEqual(Unknown);
                });
            @class.Type.ShouldEqual(Unknown);

            var typeRegistry = new TypeRegistry();
            typeRegistry.Add(@class);
            var typeChecker = new TypeChecker(typeRegistry);
            var typedClass = typeChecker.TypeCheck(@class, Scope());

            typedClass.Methods.ShouldList(
                even =>
                {
                    even.Name.Identifier.ShouldEqual("Even");
                    even.Type.ToString().ShouldEqual("System.Func<int, bool>");
                    even.Body.Type.ShouldEqual(Boolean);
                },
                odd =>
                {
                    odd.Name.Identifier.ShouldEqual("Odd");
                    odd.Type.ToString().ShouldEqual("System.Func<int, bool>");
                    odd.Body.Type.ShouldEqual(Boolean);
                },
                test =>
                {
                    test.Name.Identifier.ShouldEqual("Test");
                    test.Type.ToString().ShouldEqual("System.Func<int>");
                    test.Body.Type.ShouldEqual(Integer);
                });
            typedClass.Type.ShouldEqual(constructorReturningFoo);
        }

        public void FailsTypeCheckingWhenMethodsFailTypeChecking()
        {
            ShouldFailTypeChecking("class Foo { int A() {0} int B() {true+0} }").WithError("Type mismatch: expected int, found bool.", 1, 38);

            ShouldFailTypeChecking("class Foo { int A() { int x = 0; int x = 1; x } }").WithError("Duplicate identifier: x", 1, 38);

            ShouldFailTypeChecking("class Foo { int A() {(1)()} }").WithError("Attempted to call a noncallable object.", 1, 23);

            ShouldFailTypeChecking("class Foo { int Square(int x) {x*x} int Mismatch() {Square(1, 2)} }").WithError("Type mismatch: expected System.Func<int, int>, found System.Func<int, int, int>.", 1, 53);

            ShouldFailTypeChecking("class Foo { int A() {Square(2)} }")
                .WithErrors(
                    error => error.ShouldEqual("Reference to undefined identifier: Square", 1, 22),
                    error => error.ShouldEqual("Attempted to call a noncallable object.", 1, 22));
        }

        public void FailsTypeCheckingWhenMethodNamesAreNotUnique()
        {
            ShouldFailTypeChecking("class Foo { int A() {0} bool B() {true} int A() {1} }").WithError("Duplicate identifier: A", 1, 45);
        }

        private Vector<CompilerError> ShouldFailTypeChecking(string source, params TypeMapping[] symbols)
        {
            var @class = Parse(source);

            var typeChecker = new TypeChecker();
            typeChecker.TypeCheck(@class, Scope(symbols));
            typeChecker.HasErrors.ShouldBeTrue();

            return typeChecker.Errors;
        }
    }
}