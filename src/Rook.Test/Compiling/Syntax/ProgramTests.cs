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
        public void ParsesZeroOrMoreFunctions()
        {
            Parses(" \t\r\n").IntoTree("");
            Parses(" \t\r\n int life() 42; int universe() 42; int everything() 42; \t\r\n")
                .IntoTree("int life() 42\r\n\r\nint universe() 42\r\n\r\nint everything() 42");
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

            program.Functions.ElementAt(0).Type.ShouldBeNull();
            ((If)program.Functions.ElementAt(0).Body).Type.ShouldBeNull();
            program.Functions.ElementAt(1).Type.ShouldBeNull();
            ((If)program.Functions.ElementAt(1).Body).Type.ShouldBeNull();
            program.Functions.ElementAt(2).Type.ShouldBeNull();
            ((If)program.Functions.ElementAt(2).Body).Type.ShouldBeNull();

            typedProgram.Functions.ElementAt(0).Type.ToString().ShouldEqual("System.Func<int, bool>");
            ((If)typedProgram.Functions.ElementAt(0).Body).Type.ShouldEqual(Boolean);
            typedProgram.Functions.ElementAt(1).Type.ToString().ShouldEqual("System.Func<int, bool>");
            ((If)typedProgram.Functions.ElementAt(1).Body).Type.ShouldEqual(Boolean);
            typedProgram.Functions.ElementAt(2).Type.ToString().ShouldEqual("System.Func<int>");
            ((If)typedProgram.Functions.ElementAt(2).Body).Type.ShouldEqual(Integer);
        }

        [Fact]
        public void FailsValidationWhenFunctionsFailValidation()
        {
            AssertTypeCheckError(
                1, 24,
                "Type mismatch: expected int, found bool.",
                "int a() 0; int b() true+0; int Main() 1;");

            AssertTypeCheckError(
                1, 29,
                "Duplicate identifier: x",
                "int Main() { int x = 0; int x = 1; x; };");

            AssertTypeCheckError(
                1, 13,
                "Attempted to call a noncallable object.",
                "int Main() (1)();");

            AssertTypeCheckError(
                1, 35,
                "Type mismatch: expected System.Func<int, int>, found System.Func<int, int, int>.",
                "int Square(int x) x*x; int Main() Square(1, 2);");

            AssertTypeCheckError(
                1, 12,
                "Reference to undefined identifier: Square",
                "int Main() Square(2);");
        }

        [Fact]
        public void FailsValidationWhenFunctionNamesAreNotUnique()
        {
            AssertTypeCheckError(1, 27, "Duplicate identifier: a", "int a() 0; int b() 1; int a() 2; int Main() 1;");
        }

        private void AssertTypeCheckError(int line, int column, string expectedMessage, string source)
        {
            var expectedPosition = new Position(line, column);
            AssertTypeCheckError(Parse(source).WithTypes(), expectedPosition, expectedMessage);
        }
    }
}