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
        public void AreAlwaysFullyTyped()
        {
            var constructorReturningFoo = NamedType.Constructor(new NamedType("Foo"));

            var @class = Parse("class Foo { }");
            @class.Type.ShouldEqual(constructorReturningFoo);

            var typedClass = @class.WithTypes(Environment()).Syntax;
            typedClass.Type.ShouldEqual(constructorReturningFoo);
            typedClass.ShouldBeSameAs(@class);
        }

        private DataType Type(string source, params TypeMapping[] symbols)
        {
            return TypeChecking(source, symbols).Syntax.Type;
        }

        private TypeChecked<Class> TypeChecking(string source, params TypeMapping[] symbols)
        {
            return Parse(source).WithTypes(Environment(symbols));
        }
    }
}