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
        public void FailsTypeCheckingForTypeNameNotInScope()
        {
            TypeChecking("new Foo()").ShouldFail("Reference to undefined identifier: Foo", 1, 5);
        }

        [Fact]
        public void HasATypeEqualToThatOfTheTypeBeingConstructed()
        {
            var type = new NamedType("Foo");
            Type("new Foo()", Foo => type).ShouldEqual(type);
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var type = new NamedType("Foo");

            var @new = (New)Parse("new Foo()");
            @new.Type.ShouldBeNull();
            @new.TypeName.Type.ShouldBeNull();

            var typedNew = (New)@new.WithTypes(Environment(Foo => type)).Syntax;
            typedNew.Type.ShouldEqual(type);
            typedNew.TypeName.Type.ShouldEqual(type);
        }
    }
}