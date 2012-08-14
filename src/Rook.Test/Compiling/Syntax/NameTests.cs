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
            using (TypeVariable.TestFactory())
            {
                Type("foo", foo => new TypeVariable(0)).ShouldEqual(new TypeVariable(2));
            }

            using (TypeVariable.TestFactory())
            {
                var expectedTypeAfterLookup = new NamedType("A", new TypeVariable(2), new TypeVariable(3), new NamedType("B", new TypeVariable(2), new TypeVariable(3)));
                var definedType = new NamedType("A", new TypeVariable(0), new TypeVariable(1), new NamedType("B", new TypeVariable(0), new TypeVariable(1)));
                Type("foo", foo => definedType).ShouldEqual(expectedTypeAfterLookup);
            }
        }

        [Fact]
        public void HasATypeInWhichOnlyGenericTypeVariablesAreFreshenedOnEachScopeLookup()
        {
            using (TypeVariable.TestFactory())
            {
                //Prevent type '1' from being freshened on type lookup by marking it as non-generic:
                var typeVariable0 = TypeVariable.CreateGeneric();
                var typeVariable1 = TypeVariable.CreateNonGeneric();

                var expectedTypeAfterLookup = new NamedType("A", new TypeVariable(4), typeVariable1, new NamedType("B", new TypeVariable(4), typeVariable1));
                var definedType = new NamedType("A", typeVariable0, typeVariable1, new NamedType("B", typeVariable0, typeVariable1));

                var typeChecker = new TypeChecker();
                var globalScope = new GlobalScope();
                var localScope = new LocalScope(globalScope);
                localScope.Bind("foo", definedType);

                Type("foo", localScope, typeChecker).ShouldEqual(expectedTypeAfterLookup);
            }
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
            ShouldFailTypeChecking("foo").WithError("Reference to undefined identifier: foo", 1, 1);
        }
    }
}