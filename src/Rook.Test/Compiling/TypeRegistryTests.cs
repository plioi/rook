using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Should;

namespace Rook.Compiling
{
    [Facts]
    public class TypeRegistryTests
    {
        private readonly TypeRegistry typeRegistry;

        public TypeRegistryTests()
        {
            typeRegistry = new TypeRegistry();
        }

        public void ShouldGetNullForUnregisteredTypes()
        {
            var bogusType = typeRegistry.TypeOf(new TypeName("ThisTypeDoesNotExist"));
            bogusType.ShouldBeNull();
        }

        public void ShouldGetKeywordTypes()
        {
            typeRegistry.TypeOf(TypeName.Integer).ShouldEqual("System.Int32", "int");
            typeRegistry.TypeOf(TypeName.Boolean).ShouldEqual("System.Boolean", "bool");
            typeRegistry.TypeOf(TypeName.String).ShouldEqual("System.String", "string");
            typeRegistry.TypeOf(TypeName.Void).ShouldEqual("Rook.Core.Void", "Rook.Core.Void");
        }

        public void ShouldGetTypesForRegisteredClasses()
        {
            var math = "class Math { int Square(int x) x*x; bool Zero(int x) x==0; }".ParseClass();

            typeRegistry.Add(math);
            typeRegistry.TypeOf(new TypeName("Math")).ShouldEqual(new NamedType(math));
        }
    }
}