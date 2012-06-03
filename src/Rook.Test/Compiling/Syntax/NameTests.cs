using Rook.Compiling.Types;
using Should;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class NameTests : ExpressionTests
    {
        [Fact]
        public void CanBeIdentifier()
        {
            Parses("a").IntoTree("a");
            Parses("abc").IntoTree("abc");
        }

        [Fact]
        public void HasATypeProvidedByTheGivenScope()
        {
            Type("foo", foo => Boolean, bar => Integer).ShouldEqual(Boolean);
            Type("bar", foo => Boolean, bar => Integer).ShouldEqual(Integer);
        }

        [Fact]
        public void HasATypeInWhichTypeVariablesAreFreshenedOnEachScopeLookup()
        {
            Type("foo", foo => new TypeVariable(0)).ShouldEqual(new TypeVariable(2));

            var expectedTypeAfterLookup = new NamedType("A", new TypeVariable(2), new TypeVariable(3), new NamedType("B", new TypeVariable(2), new TypeVariable(3)));
            var definedType = new NamedType("A", new TypeVariable(0), new TypeVariable(1), new NamedType("B", new TypeVariable(0), new TypeVariable(1)));
            Type("foo", foo => definedType).ShouldEqual(expectedTypeAfterLookup);
        }

        [Fact]
        public void HasATypeInWhichOnlyGenericTypeVariablesAreFreshenedOnEachScopeLookup()
        {
            //Prevents type '20' from being freshened on type lookup by marking it as non-generic in the scope:

            var expectedTypeAfterLookup = new NamedType("A", new TypeVariable(2), new TypeVariable(20), new NamedType("B", new TypeVariable(2), new TypeVariable(20)));
            var definedType = new NamedType("A", new TypeVariable(0), new TypeVariable(20), new NamedType("B", new TypeVariable(0), new TypeVariable(20)));

            var rootScope = new Scope();
            var scope = Compiling.Scope.CreateScopeWithBuiltins(rootScope);
            scope.TreatAsNonGeneric(new[] { new TypeVariable(20) });
            scope["foo"] = definedType;

            Type("foo", scope).ShouldEqual(expectedTypeAfterLookup);
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var node = (Name)Parse("foo");
            node.Type.ShouldBeNull();

            var typedNode = (Name)node.WithTypes(Scope(foo => Boolean), new TypeUnifier()).Syntax;
            typedNode.Type.ShouldEqual(Boolean);
        }

        [Fact]
        public void FailsTypeCheckingForIdentifiersNotInScope()
        {
            TypeChecking("foo").ShouldFail("Reference to undefined identifier: foo", 1, 1);
        }
    }
}