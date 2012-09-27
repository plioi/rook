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

        public void ShouldRegisterParsedClasses()
        {
            var math = "class Math { int Square(int x) x*x; bool Zero(int x) x==0; }".ParseClass();

            typeRegistry.Add(math);
            typeRegistry.TypeOf(new TypeName("Math")).ShouldEqual(new NamedType("Math"));
        }

        public void ShouldDiscoverUnregisteredTypesViaReflection()
        {
            var sample = typeRegistry.TypeOf(new TypeName("System.Int32"));
            sample.Name.ShouldEqual("System.Int32");
            sample.IsGeneric.ShouldBeFalse();
            sample.ToString().ShouldEqual("int");
        }

        public void ShouldReturnNullForUnregisteredTypesWhenReflectionCannotFindThem()
        {
            var bogusType = typeRegistry.TypeOf(new TypeName("ThisTypeDoesNotExist"));
            bogusType.ShouldBeNull();
        }
    }
}