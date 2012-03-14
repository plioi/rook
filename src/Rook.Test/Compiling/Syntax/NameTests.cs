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
        public void HasATypeProvidedByTheEnvironmentInScope()
        {
            AssertType(Boolean, "foo", foo => Boolean, bar => Integer);
            AssertType(Integer, "bar", foo => Boolean, bar => Integer);
        }

        [Fact]
        public void HasATypeInWhichTypeVariablesAreFreshenedOnEachEnvironmentLookup()
        {
            AssertType(new TypeVariable(17), "foo", foo => new TypeVariable(1));

            NamedType expectedTypeAfterLookup = new NamedType("A", new TypeVariable(17), new TypeVariable(18), new NamedType("B", new TypeVariable(17), new TypeVariable(18)));
            NamedType definedType = new NamedType("A", new TypeVariable(1), new TypeVariable(2), new NamedType("B", new TypeVariable(1), new TypeVariable(2)));
            AssertType(expectedTypeAfterLookup, "foo", foo => definedType);
        }

        [Fact]
        public void HasATypeInWhichOnlyGenericTypeVariablesAreFreshenedOnEachEnvironmentLookup()
        {
            //Prevents type '2' from being freshened on type lookup by marking it as non-generic in the environment:

            NamedType expectedTypeAfterLookup = new NamedType("A", new TypeVariable(17), new TypeVariable(2), new NamedType("B", new TypeVariable(17), new TypeVariable(2)));
            NamedType definedType = new NamedType("A", new TypeVariable(1), new TypeVariable(2), new NamedType("B", new TypeVariable(1), new TypeVariable(2)));

            var environment = new Environment();
            environment.TreatAsNonGeneric(new[] { new TypeVariable(2) });
            environment["foo"] = definedType;

            AssertType(expectedTypeAfterLookup, "foo", environment);
        }

        [Fact]
        public void CanCreateFullyTypedInstance()
        {
            var node = (Name)Parse("foo");
            node.Type.ShouldBeNull();

            var typedNode = (Name)node.WithTypes(Environment(foo => Boolean)).Syntax;
            typedNode.Type.ShouldEqual(Boolean);
        }

        [Fact]
        public void FailsTypeCheckingForIdentifiersNotInScope()
        {
            AssertTypeCheckError(1, 1, "Reference to undefined identifier: foo", "foo");
        }
    }
}