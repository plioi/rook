using NUnit.Framework;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    [TestFixture]
    public sealed class NameSpec : ExpressionSpec
    {
        [Test]
        public void CanBeIdentifier()
        {
            Parses("a").IntoTree("a");
            Parses("abc").IntoTree("abc");
        }

        [Test]
        public void HasATypeProvidedByTheEnvironmentInScope()
        {
            AssertType(Boolean, "foo", foo => Boolean, bar => Integer);
            AssertType(Integer, "bar", foo => Boolean, bar => Integer);
        }

        [Test]
        public void HasATypeInWhichTypeVariablesAreFreshenedOnEachEnvironmentLookup()
        {
            AssertType(new TypeVariable(17), "foo", foo => new TypeVariable(1));

            NamedType expectedTypeAfterLookup = NamedType.Create("A", new TypeVariable(17), new TypeVariable(18), NamedType.Create("B", new TypeVariable(17), new TypeVariable(18)));
            NamedType definedType = NamedType.Create("A", new TypeVariable(1), new TypeVariable(2), NamedType.Create("B", new TypeVariable(1), new TypeVariable(2)));
            AssertType(expectedTypeAfterLookup, "foo", foo => definedType);
        }

        [Test]
        public void HasATypeInWhichOnlyGenericTypeVariablesAreFreshenedOnEachEnvironmentLookup()
        {
            //Prevents type '2' from being freshened on type lookup by marking it as non-generic in the environment:

            NamedType expectedTypeAfterLookup = NamedType.Create("A", new TypeVariable(17), new TypeVariable(2), NamedType.Create("B", new TypeVariable(17), new TypeVariable(2)));
            NamedType definedType = NamedType.Create("A", new TypeVariable(1), new TypeVariable(2), NamedType.Create("B", new TypeVariable(1), new TypeVariable(2)));

            var environment = new Environment();
            environment.TreatAsNonGeneric(new[] { new TypeVariable(2) });
            environment["foo"] = definedType;

            AssertType(expectedTypeAfterLookup, "foo", environment);
        }

        [Test]
        public void CanCreateFullyTypedInstance()
        {
            var node = (Name)Parse("foo");
            node.Type.ShouldBeNull();

            var typedNode = (Name)node.WithTypes(Environment(foo => Boolean)).Syntax;
            typedNode.Type.ShouldBeTheSameAs(Boolean);
        }

        [Test]
        public void FailsTypeCheckingForIdentifiersNotInScope()
        {
            AssertTypeCheckError(1, 1, "Reference to undefined identifier: foo", "foo");
        }
    }
}