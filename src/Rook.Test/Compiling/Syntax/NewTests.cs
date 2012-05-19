using Parsley;
using Rook.Compiling.Types;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class NewTests : ExpressionTests
    {
        [Fact]
        public void InvokesConstructorByTypeName()
        {
            FailsToParse("new").AtEndOfInput().WithMessage("(1, 4): identifier expected");
            FailsToParse("new Foo").AtEndOfInput().WithMessage("(1, 8): ( expected");
            FailsToParse("new Foo(").AtEndOfInput().WithMessage("(1, 9): ) expected");
            Parses("new Foo()").IntoTree("(new Foo())");
        }

        [Fact]
        public void HasATypeEqualToThatOfTheTypeBeingConstructed()
        {
            var constructedType = new NamedType("Foo");
            var constructorType = NamedType.Constructor(constructedType);
            Type("new Foo()", Foo => constructorType).ShouldEqual(constructedType);
        }

        [Fact]
        public void FailsTypeCheckingForTypeNameNotInScope()
        {
            TypeChecking("new Foo()").ShouldFail("Reference to undefined identifier: Foo", 1, 5);
        }

        [Fact]
        public void FailsTypeCheckingForNamesThatAreNotConstructorNames()
        {
            TypeChecking("new Foo()", Foo => Integer).ShouldFail("Cannot construct 'Foo' because it is not a type.", 1, 5);
            TypeChecking("new Foo()", Foo => NamedType.Function(Integer)).ShouldFail("Cannot construct 'Foo' because it is not a type.", 1, 5);
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var constructedType = new NamedType("Foo");
            var constructorType = NamedType.Constructor(constructedType);

            var @new = (New)Parse("new Foo()");
            @new.Type.ShouldBeNull();
            @new.TypeName.Type.ShouldBeNull();

            var typedNew = (New)@new.WithTypes(Environment(Foo => constructorType)).Syntax;
            typedNew.Type.ShouldEqual(constructedType);
            typedNew.TypeName.Type.ShouldEqual(constructorType);
        }
    }
}