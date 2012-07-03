using System.Linq;
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
            //Prevents type '1' from being freshened on type lookup by marking it as non-generic in the scope:

            var expectedTypeAfterLookup = new NamedType("A", new TypeVariable(2), new TypeVariable(1), new NamedType("B", new TypeVariable(2), new TypeVariable(1)));
            var definedType = new NamedType("A", new TypeVariable(0), new TypeVariable(1), new NamedType("B", new TypeVariable(0), new TypeVariable(1)));

            var unifier = new TypeUnifier();
            var scope = Compiling.Scope.CreateRoot(unifier.CreateTypeVariable, Enumerable.Empty<TypeMemberBinding>());
            scope.TreatAsNonGeneric(new[] { new TypeVariable(1) });
            scope["foo"] = definedType;

            Type("foo", scope, unifier).ShouldEqual(expectedTypeAfterLookup);
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var name = (Name)Parse("foo");
            name.Type.ShouldBeNull();

            var typedName = WithTypes(name, foo => Boolean);
            typedName.Type.ShouldEqual(Boolean);
        }

        [Fact]
        public void FailsTypeCheckingForIdentifiersNotInScope()
        {
            TypeChecking("foo").ShouldFail("Reference to undefined identifier: foo", 1, 1);
        }
    }
}