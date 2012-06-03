using Parsley;
using Rook.Compiling.Types;
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

            var unifier = new TypeUnifier();
            var typeCheckedClass = @class.WithTypes(Scope(unifier), unifier);
            var typedClass = typeCheckedClass.Syntax;

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

        [Fact]
        public void FailsTypeCheckingWhenMethodsFailTypeChecking()
        {
            TypeChecking("class Foo { int A() 0; int B() true+0; }").ShouldFail("Type mismatch: expected int, found bool.", 1, 36);

            TypeChecking("class Foo { int A() { int x = 0; int x = 1; x; }; }").ShouldFail("Duplicate identifier: x", 1, 38);

            TypeChecking("class Foo { int A() (1)(); }").ShouldFail("Attempted to call a noncallable object.", 1, 22);

            TypeChecking("class Foo { int Square(int x) x*x; int Mismatch() Square(1, 2); }").ShouldFail("Type mismatch: expected System.Func<int, int>, found System.Func<int, int, int>.", 1, 51);

            TypeChecking("class Foo { int A() Square(2); }").ShouldFail("Reference to undefined identifier: Square", 1, 21);
        }

        [Fact]
        public void FailsTypeCheckingWhenMethodNamesAreNotUnique()
        {
            TypeChecking("class Foo { int A() 0; bool B() true; int A() 1; }").ShouldFail("Duplicate identifier: A", 1, 43);
        }

        [Fact]
        public void FailsTypeCheckingWhenMethodNamesShadowSurroundingScope()
        {
            var pointConstructor = NamedType.Constructor(new NamedType("Point"));

            TypeChecking("class Foo { int A() 0; int Point() 2; }", Point => pointConstructor)
                .ShouldFail("Duplicate identifier: Point", 1, 28);
        }

        private DataType Type(string source, params TypeMapping[] symbols)
        {
            return TypeChecking(source, symbols).Syntax.Type;
        }

        private TypeChecked<Class> TypeChecking(string source, params TypeMapping[] symbols)
        {
            var unifier = new TypeUnifier();
            return Parse(source).WithTypes(Scope(unifier, symbols), unifier);
        }
    }
}