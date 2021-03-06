﻿using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;
using Should;

namespace Rook.Compiling.Syntax
{
    public class CompilationUnitTests : SyntaxTreeTests<CompilationUnit>
    {
        protected override Parser<CompilationUnit> Parser { get { return RookGrammar.CompilationUnit; } }

        public void ParsesZeroOrMoreClasses()
        {
            Parses(" \t\r\n").IntoTree("");
            Parses(" \t\r\n class Foo {} class Bar {} class Baz {} \t\r\n")
                .IntoTree("class Foo {} class Bar {} class Baz {}");
        }

        public void ParsesZeroOrMoreFunctions()
        {
            Parses(" \t\r\n").IntoTree("");
            Parses(" \t\r\n int life() {42} int universe() {42} int everything() {42} \t\r\n")
                .IntoTree("int life() {42} int universe() {42} int everything() {42}");
        }

        public void DemandsClassesAppearBeforeFunctions()
        {
            Parses(" \t\r\n class Foo {} class Bar {} int life() {42} int universe() {42} int everything() {42} \t\r\n")
                .IntoTree("class Foo {} class Bar {} int life() {42} int universe() {42} int everything() {42}");
            FailsToParse("int square(int x) {x*x} class Foo { }").LeavingUnparsedTokens("class", "Foo", "{", "}").WithMessage("(1, 25): end of input expected");
        }

        public void DemandsEndOfInputAfterLastValidClassOrFunction()
        {
            FailsToParse("int life() {42} int univ").AtEndOfInput().WithMessage("(1, 25): ( expected");
            FailsToParse("class Foo { } class").AtEndOfInput().WithMessage("(1, 20): identifier expected");
        }

        public void TypesAllClassesAndFunctions()
        {
            var compilationUnit = Parse(
                @"class Foo { }
                  class Bar { }
                  bool Even(int n) {if (n==0) true else Odd(n-1)}
                  bool Odd(int n) {if (n==0) false else Even(n-1)}
                  int Main() {if (Even(4)) 0 else 1}");


            var fooType = new NamedType(compilationUnit.Classes.Single(c => c.Name.Identifier == "Foo"));
            var barType = new NamedType(compilationUnit.Classes.Single(c => c.Name.Identifier == "Bar"));

            var fooConstructorType = NamedType.Constructor(fooType);
            var barConstructorType = NamedType.Constructor(barType);

            var typeChecker = new TypeChecker();
            var typedCompilationUnit = typeChecker.TypeCheck(compilationUnit);

            compilationUnit.Classes.ShouldList(
                foo => foo.Type.ShouldEqual(Unknown),
                bar => bar.Type.ShouldEqual(Unknown));

            compilationUnit.Functions.ShouldList(
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
                main =>
                {
                    main.Name.Identifier.ShouldEqual("Main");
                    main.Type.ShouldEqual(Unknown);
                    main.Body.Type.ShouldEqual(Unknown);
                });

            typedCompilationUnit.Classes.ShouldList(
                foo => foo.Type.ShouldEqual(fooConstructorType),
                bar => bar.Type.ShouldEqual(barConstructorType));

            typedCompilationUnit.Functions.ShouldList(
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

        public void FailsValidationWhenFunctionsFailValidation()
        {
            ShouldFailTypeChecking("int a() {0} int b() {true+0} int Main() {1}").WithError("Type mismatch: expected int, found bool.", 1, 26);

            ShouldFailTypeChecking("int Main() { int x = 0; int x = 1; x }").WithError("Duplicate identifier: x", 1, 29);

            ShouldFailTypeChecking("int Main() {(1)()}").WithError("Attempted to call a noncallable object.", 1, 14);

            ShouldFailTypeChecking("int Square(int x) {x*x} int Main() {Square(1, 2)}").WithError("Type mismatch: expected System.Func<int, int>, found System.Func<int, int, int>.", 1, 37);

            ShouldFailTypeChecking("int Main() {Square(2)}")
                .WithErrors(
                    error => error.ShouldEqual("Reference to undefined identifier: Square", 1, 13),
                    error => error.ShouldEqual("Attempted to call a noncallable object.", 1, 13));
        }

        public void FailsValidationWhenClassesFailValidation()
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

        public void FailsValidationWhenFunctionAndClassNamesAreNotUnique()
        {
            ShouldFailTypeChecking("int a() {0} int b() {1} int a() {2} int Main() {1}").WithError("Duplicate identifier: a", 1, 29);
            ShouldFailTypeChecking("class Foo { } class Bar { } class Foo { }").WithError("Duplicate identifier: Foo", 1, 29);
            ShouldFailTypeChecking("class Zero { } int Zero() {0}").WithError("Duplicate identifier: Zero", 1, 20);
        }

        private Vector<CompilerError> ShouldFailTypeChecking(string source)
        {
            var compilationUnit = Parse(source);

            var typeChecker = new TypeChecker();
            typeChecker.TypeCheck(compilationUnit);
            typeChecker.HasErrors.ShouldBeTrue();

            return typeChecker.Errors;
        }
    }
}