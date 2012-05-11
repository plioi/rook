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
            Parses("class Foo").IntoTree("class Foo");
        }

        [Fact]
        public void HasATypeCorrespondingWithTheDefaultConstructorFunction()
        {
            var constructorReturningFoo = NamedType.Function(new NamedType("Foo"));

            Type("class Foo").ShouldEqual(constructorReturningFoo);
        }

        [Fact]
        public void AreAlwaysFullyTyped()
        {
            var constructorReturningFoo = NamedType.Function(new NamedType("Foo"));

            var @class = Parse("class Foo");
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