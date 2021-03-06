using Parsley;
using Rook.Compiling.Types;
using Should;

namespace Rook.Compiling.Syntax
{
    public class NewTests : ExpressionTests
    {
        public void InvokesConstructorByTypeName()
        {
            FailsToParse("new").AtEndOfInput().WithMessage("(1, 4): identifier expected");
            FailsToParse("new Foo").AtEndOfInput().WithMessage("(1, 8): ( expected");
            FailsToParse("new Foo(").AtEndOfInput().WithMessage("(1, 9): ) expected");
            Parses("new Foo()").IntoTree("(new Foo())");
        }

        public void HasATypeEqualToThatOfTheTypeBeingConstructed()
        {
            var constructedType = new NamedType("class Foo { }".ParseClass());
            var constructorType = NamedType.Constructor(constructedType);
            Type("new Foo()", Foo => constructorType).ShouldEqual(constructedType);
        }

        public void FailsTypeCheckingForTypeNameNotInScope()
        {
            ShouldFailTypeChecking("new Foo()")
                .WithErrors(
                    error => error.ShouldEqual("Reference to undefined identifier: Foo", 1, 5),
                    error => error.ShouldEqual("Cannot construct 'Foo' because it is not a type.", 1, 5));
        }

        public void FailsTypeCheckingForNamesThatAreNotConstructorNames()
        {
            ShouldFailTypeChecking("new Foo()", Foo => Integer).WithError("Cannot construct 'Foo' because it is not a type.", 1, 5);
            ShouldFailTypeChecking("new Foo()", Foo => NamedType.Function(Integer)).WithError("Cannot construct 'Foo' because it is not a type.", 1, 5);
        }

        public void CanCreateFullyTypedInstance()
        {
            var constructedType = new NamedType("class Foo { }".ParseClass());
            var constructorType = NamedType.Constructor(constructedType);

            var @new = (New)Parse("new Foo()");
            @new.Type.ShouldEqual(Unknown);
            @new.TypeName.Type.ShouldEqual(Unknown);

            var typedNew = WithTypes(@new, Foo => constructorType);
            typedNew.Type.ShouldEqual(constructedType);
            typedNew.TypeName.Type.ShouldEqual(constructorType);
        }
    }
}