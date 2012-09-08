using Should;

namespace Rook.Compiling.Types
{
    [Facts]
    public class DataTypeTests
    {
        public void CanFreshenGenericTypeVariables()
        {
            using (TypeVariable.TestFactory())
            {
                //Prevent type '1' from being freshened by marking it as non-generic:
                var typeVariable0 = TypeVariable.CreateGeneric();
                var typeVariable1 = TypeVariable.CreateNonGeneric();

                var expectedTypeAfterLookup = new NamedType("A", new TypeVariable(2), typeVariable1, new NamedType("B", new TypeVariable(2), typeVariable1));
                var definedType = new NamedType("A", typeVariable0, typeVariable1, new NamedType("B", typeVariable0, typeVariable1));

                definedType.FreshenGenericTypeVariables().ShouldEqual(expectedTypeAfterLookup);
            }
        }
    }
}