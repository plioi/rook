using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class ClassTests : SyntaxTreeTests<Class>
    {
        protected override Parser<Class> Parser { get { return RookGrammar.Class; } }

        [Fact]
        public void ParsesEmptyClassDeclarations()
        {
            FailsToParse("").AtEndOfInput().WithMessage("(1, 1): class expected");
            FailsToParse("class").AtEndOfInput().WithMessage("(1, 6): identifier expected");
            FailsToParse("class Foo").AtEndOfInput().WithMessage("(1, 10): { expected");
            FailsToParse("class Foo {").AtEndOfInput().WithMessage("(1, 12): } expected");
            Parses("class Foo {}").IntoTree("class Foo {}");
        }

        [Fact]
        public void ParsesMethods()
        {
            Parses("class Hitchhiker {int life() 42; int universe() 42; int everything() 42;}")
                .IntoTree("class Hitchhiker {int life() 42; int universe() 42; int everything() 42}");
        }

        [Fact]
        public void DemandsEndOfClassAfterLastValidMethod()
        {
            FailsToParse("class Hitchhiker { int life() 42;").AtEndOfInput().WithMessage("(1, 34): } expected");
        }

        [Fact]
        public void HasATypeCorrespondingWithTheDefaultConstructor()
        {
            var constructorReturningFoo = NamedType.Constructor(new NamedType("Foo"));

            Type("class Foo { }").ShouldEqual(constructorReturningFoo);
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var constructorReturningFoo = NamedType.Constructor(new NamedType("Foo"));

            var @class = Parse(
                @"class Foo
                  {
                      bool Even(int n) if (n==0) true else Odd(n-1);
                      bool Odd(int n) if (n==0) false else Even(n-1);
                      int Test() if (Even(4)) 0 else 1;
                  }");

            @class.Methods.ShouldList(
                even =>
                {
                    even.Name.Identifier.ShouldEqual("Even");
                    even.Type.ShouldBeNull();
                    even.Body.Type.ShouldBeNull();
                },
                odd =>
                {
                    odd.Name.Identifier.ShouldEqual("Odd");
                    odd.Type.ShouldBeNull();
                    odd.Body.Type.ShouldBeNull();
                },
                test =>
                {
                    test.Name.Identifier.ShouldEqual("Test");
                    test.Type.ShouldBeNull();
                    test.Body.Type.ShouldBeNull();
                });
            @class.Type.ShouldEqual(constructorReturningFoo);

            var typeChecker = new TypeChecker();
            var typeCheckedClass = typeChecker.TypeCheck(@class, Scope());

            typeCheckedClass.Methods.ShouldList(
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
            typeCheckedClass.Type.ShouldEqual(constructorReturningFoo);
        }

        [Fact]
        public void FailsTypeCheckingWhenMethodsFailTypeChecking()
        {
            ShouldFailTypeChecking("class Foo { int A() 0; int B() true+0; }").WithError("Type mismatch: expected int, found bool.", 1, 36);

            ShouldFailTypeChecking("class Foo { int A() { int x = 0; int x = 1; x; }; }").WithError("Duplicate identifier: x", 1, 38);

            ShouldFailTypeChecking("class Foo { int A() (1)(); }").WithError("Attempted to call a noncallable object.", 1, 22);

            ShouldFailTypeChecking("class Foo { int Square(int x) x*x; int Mismatch() Square(1, 2); }").WithError("Type mismatch: expected System.Func<int, int>, found System.Func<int, int, int>.", 1, 51);

            ShouldFailTypeChecking("class Foo { int A() Square(2); }").WithError("Reference to undefined identifier: Square", 1, 21);
        }

        [Fact]
        public void FailsTypeCheckingWhenMethodNamesAreNotUnique()
        {
            ShouldFailTypeChecking("class Foo { int A() 0; bool B() true; int A() 1; }").WithError("Duplicate identifier: A", 1, 43);
        }

        [Fact]
        public void FailsTypeCheckingWhenMethodNamesShadowSurroundingScope()
        {
            var pointConstructor = NamedType.Constructor(new NamedType("Point"));

            ShouldFailTypeChecking("class Foo { int A() 0; int Point() 2; }", Point => pointConstructor)
                .WithError("Duplicate identifier: Point", 1, 28);
        }

        private DataType Type(string source, params TypeMapping[] symbols)
        {
            var @class = Parse(source);

            var typeChecker = new TypeChecker();
            var typeCheckedClass = typeChecker.TypeCheck(@class, Scope(symbols));

            typeCheckedClass.ShouldNotBeNull();
            typeChecker.HasErrors.ShouldBeFalse();

            return typeCheckedClass.Type;
        }

        private Vector<CompilerError> ShouldFailTypeChecking(string source, params TypeMapping[] symbols)
        {
            var @class = Parse(source);

            var typeChecker = new TypeChecker();
            var typeCheckedClass = typeChecker.TypeCheck(@class, Scope(symbols));

            typeCheckedClass.ShouldBeNull();
            typeChecker.HasErrors.ShouldBeTrue();

            return typeChecker.Errors;
        }
    }
}