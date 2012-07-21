using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class CompilationUnitTests : SyntaxTreeTests<CompilationUnit>
    {
        protected override Parser<CompilationUnit> Parser { get { return RookGrammar.CompilationUnit; } }

        [Fact]
        public void ParsesZeroOrMoreClasses()
        {
            Parses(" \t\r\n").IntoTree("");
            Parses(" \t\r\n class Foo {}; class Bar {}; class Baz {}; \t\r\n")
                .IntoTree("class Foo {}; class Bar {}; class Baz {}");
        }

        [Fact]
        public void ParsesZeroOrMoreFunctions()
        {
            Parses(" \t\r\n").IntoTree("");
            Parses(" \t\r\n int life() 42; int universe() 42; int everything() 42; \t\r\n")
                .IntoTree("int life() 42; int universe() 42; int everything() 42");
        }

        [Fact]
        public void DemandsClassesAppearBeforeFunctions()
        {
            Parses(" \t\r\n class Foo {}; class Bar {}; int life() 42; int universe() 42; int everything() 42; \t\r\n")
                .IntoTree("class Foo {}; class Bar {}; int life() 42; int universe() 42; int everything() 42");
            FailsToParse("int square(int x) x*x; class Foo { }").LeavingUnparsedTokens("class", "Foo", "{", "}").WithMessage("(1, 24): end of input expected");
        }

        [Fact]
        public void DemandsEndOfInputAfterLastValidClassOrFunction()
        {
            FailsToParse("int life() 42; int univ").AtEndOfInput().WithMessage("(1, 24): ( expected");
            FailsToParse("class Foo { }; class").AtEndOfInput().WithMessage("(1, 21): identifier expected");
        }

        [Fact]
        public void TypesAllClassesAndFunctions()
        {
            var fooConstructorType = NamedType.Constructor(new NamedType("Foo"));
            var barConstructorType = NamedType.Constructor(new NamedType("Bar"));

            var compilationUnit = Parse(
                @"class Foo { }
                  class Bar { }
                  bool Even(int n) if (n==0) true else Odd(n-1);
                  bool Odd(int n) if (n==0) false else Even(n-1);
                  int Main() if (Even(4)) 0 else 1;");

            var typeChecker = new TypeChecker();
            var typeCheckedCompilationUnit = typeChecker.TypeCheck(compilationUnit);

            compilationUnit.Classes.ShouldList(
                foo => foo.Type.ShouldEqual(fooConstructorType),
                bar => bar.Type.ShouldEqual(barConstructorType));

            compilationUnit.Functions.ShouldList(
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
                main =>
                {
                    main.Name.Identifier.ShouldEqual("Main");
                    main.Type.ShouldBeNull();
                    main.Body.Type.ShouldBeNull();
                });

            typeCheckedCompilationUnit.Classes.ShouldList(
                foo => foo.Type.ShouldEqual(fooConstructorType),
                bar => bar.Type.ShouldEqual(barConstructorType));

            typeCheckedCompilationUnit.Functions.ShouldList(
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
                main =>
                {
                    main.Name.Identifier.ShouldEqual("Main");
                    main.Type.ToString().ShouldEqual("System.Func<int>");
                    main.Body.Type.ShouldEqual(Integer);
                });
        }

        [Fact]
        public void FailsValidationWhenFunctionsFailValidation()
        {
            ShouldFailTypeChecking("int a() 0; int b() true+0; int Main() 1;").WithError("Type mismatch: expected int, found bool.", 1, 24);

            ShouldFailTypeChecking("int Main() { int x = 0; int x = 1; x; };").WithError("Duplicate identifier: x", 1, 29);

            ShouldFailTypeChecking("int Main() (1)();").WithError("Attempted to call a noncallable object.", 1, 13);

            ShouldFailTypeChecking("int Square(int x) x*x; int Main() Square(1, 2);").WithError("Type mismatch: expected System.Func<int, int>, found System.Func<int, int, int>.", 1, 35);

            ShouldFailTypeChecking("int Main() Square(2);").WithError("Reference to undefined identifier: Square", 1, 12);
        }

        [Fact]
        public void FailsValidationWhenClassesFailValidation()
        {
            ShouldFailTypeChecking("class Foo { int A() 0; int B() true+0; }").WithError("Type mismatch: expected int, found bool.", 1, 36);

            ShouldFailTypeChecking("class Foo { int A() { int x = 0; int x = 1; x; }; }").WithError("Duplicate identifier: x", 1, 38);

            ShouldFailTypeChecking("class Foo { int A() (1)(); }").WithError("Attempted to call a noncallable object.", 1, 22);

            ShouldFailTypeChecking("class Foo { int Square(int x) x*x; int Mismatch() Square(1, 2); }").WithError("Type mismatch: expected System.Func<int, int>, found System.Func<int, int, int>.", 1, 51);

            ShouldFailTypeChecking("class Foo { int A() Square(2); }").WithError("Reference to undefined identifier: Square", 1, 21);
        }

        [Fact]
        public void FailsValidationWhenFunctionAndClassNamesAreNotUnique()
        {
            ShouldFailTypeChecking("int a() 0; int b() 1; int a() 2; int Main() 1;").WithError("Duplicate identifier: a", 1, 27);
            ShouldFailTypeChecking("class Foo { }; class Bar { }; class Foo { }").WithError("Duplicate identifier: Foo", 1, 31);
            ShouldFailTypeChecking("class Zero { }; int Zero() 0;").WithError("Duplicate identifier: Zero", 1, 21);
        }

        private Vector<CompilerError> ShouldFailTypeChecking(string source)
        {
            var compilationUnit = Parse(source);

            var typeChecker = new TypeChecker();
            var typeCheckedCompilationUnit = typeChecker.TypeCheck(compilationUnit);

            typeCheckedCompilationUnit.ShouldBeNull();
            typeChecker.HasErrors.ShouldBeTrue();

            return typeChecker.Errors;
        }
    }
}