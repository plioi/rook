using System.Linq;
using Parsley;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class ProgramTests : SyntaxTreeTests<Program>
    {
        protected override Parser<Program> Parser { get { return RookGrammar.Program; } }

        [Fact]
        public void ParsesZeroOrMoreClasses()
        {
            Parses(" \t\r\n").IntoTree("");
            Parses(" \t\r\n class Foo; class Bar; class Baz; \t\r\n")
                .IntoTree("class Foo\r\n\r\nclass Bar\r\n\r\nclass Baz");
        }

        [Fact]
        public void ParsesZeroOrMoreFunctions()
        {
            Parses(" \t\r\n").IntoTree("");
            Parses(" \t\r\n int life() 42; int universe() 42; int everything() 42; \t\r\n")
                .IntoTree("int life() 42\r\n\r\nint universe() 42\r\n\r\nint everything() 42");
        }

        [Fact]
        public void DemandsClassesAppearBeforeFunctions()
        {
            Parses(" \t\r\n class Foo; class Bar; int life() 42; int universe() 42; int everything() 42; \t\r\n")
                .IntoTree("class Foo\r\n\r\nclass Bar\r\n\r\nint life() 42\r\n\r\nint universe() 42\r\n\r\nint everything() 42");
            FailsToParse("int square(x) x*x; class Foo").LeavingUnparsedTokens("class", "Foo").WithMessage("(1, 20): end of input expected");
        }

        [Fact]
        public void DemandsEndOfInputAfterLastValidFunction()
        {
            FailsToParse("int life() 42; int univ").AtEndOfInput().WithMessage("(1, 24): ( expected");
        }

        [Fact]
        public void ParsesAndTypesMutuallyRecursivePrograms()
        {
            var program = Parse(
                @"bool even(int n) if (n==0) true else odd(n-1);
                  bool odd(int n) if (n==0) false else even(n-1);
                  int Main() if (even(4)) 0 else 1;");

            var typeCheckedProgram = program.WithTypes();
            var typedProgram = typeCheckedProgram.Syntax;

            program.Functions.ShouldList(
                even =>
                {
                    even.Type.ShouldBeNull();
                    even.Body.Type.ShouldBeNull();
                },
                odd =>
                {
                    odd.Type.ShouldBeNull();
                    odd.Body.Type.ShouldBeNull();
                },
                main =>
                {
                    main.Type.ShouldBeNull();
                    main.Body.Type.ShouldBeNull();
                });

            typedProgram.Functions.ShouldList(
                even =>
                {
                    even.Type.ToString().ShouldEqual("System.Func<int, bool>");
                    even.Body.Type.ShouldEqual(Boolean);
                },
                odd =>
                {
                    odd.Type.ToString().ShouldEqual("System.Func<int, bool>");
                    odd.Body.Type.ShouldEqual(Boolean);
                },
                main =>
                {
                    main.Type.ToString().ShouldEqual("System.Func<int>");
                    main.Body.Type.ShouldEqual(Integer);
                });
        }

        [Fact]
        public void FailsValidationWhenFunctionsFailValidation()
        {
            TypeChecking("int a() 0; int b() true+0; int Main() 1;").ShouldFail("Type mismatch: expected int, found bool.", 1, 24);

            TypeChecking("int Main() { int x = 0; int x = 1; x; };").ShouldFail("Duplicate identifier: x", 1, 29);

            TypeChecking("int Main() (1)();").ShouldFail("Attempted to call a noncallable object.", 1, 13);

            TypeChecking("int Square(int x) x*x; int Main() Square(1, 2);").ShouldFail("Type mismatch: expected System.Func<int, int>, found System.Func<int, int, int>.", 1, 35);

            TypeChecking("int Main() Square(2);").ShouldFail("Reference to undefined identifier: Square", 1, 12);
        }

        [Fact]
        public void FailsValidationWhenFunctionNamesAreNotUnique()
        {
            TypeChecking("int a() 0; int b() 1; int a() 2; int Main() 1;").ShouldFail("Duplicate identifier: a", 1, 27);
        }

        private TypeChecked<Program> TypeChecking(string source)
        {
            return Parse(source).WithTypes();
        }
    }
}