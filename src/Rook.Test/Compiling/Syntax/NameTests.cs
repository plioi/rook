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
            Type("foo", foo => Boolean, bar => Integer).ShouldEqual(Boolean);
            Type("bar", foo => Boolean, bar => Integer).ShouldEqual(Integer);
        }

        [Fact]
        public void HasATypeInWhichTypeVariablesAreFreshenedOnEachEnvironmentLookup()
        {
            Type("foo", foo => new TypeVariable(1)).ShouldEqual(new TypeVariable(16));

            var expectedTypeAfterLookup = new NamedType("A", new TypeVariable(16), new TypeVariable(17), new NamedType("B", new TypeVariable(16), new TypeVariable(17)));
            var definedType = new NamedType("A", new TypeVariable(1), new TypeVariable(2), new NamedType("B", new TypeVariable(1), new TypeVariable(2)));
            Type("foo", foo => definedType).ShouldEqual(expectedTypeAfterLookup);
        }

        [Fact]
        public void HasATypeInWhichOnlyGenericTypeVariablesAreFreshenedOnEachEnvironmentLookup()
        {
            //Prevents type '2' from being freshened on type lookup by marking it as non-generic in the environment:

            var expectedTypeAfterLookup = new NamedType("A", new TypeVariable(16), new TypeVariable(2), new NamedType("B", new TypeVariable(16), new TypeVariable(2)));
            var definedType = new NamedType("A", new TypeVariable(1), new TypeVariable(2), new NamedType("B", new TypeVariable(1), new TypeVariable(2)));

            var rootEnvironment = new Environment();
            var environment = Compiling.Environment.CreateEnvironmentWithBuiltins(rootEnvironment);
            environment.TreatAsNonGeneric(new[] { new TypeVariable(2) });
            environment["foo"] = definedType;

            Type("foo", environment).ShouldEqual(expectedTypeAfterLookup);
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
            TypeChecking("foo").ShouldFail("Reference to undefined identifier: foo", 1, 1);
        }
    }
}