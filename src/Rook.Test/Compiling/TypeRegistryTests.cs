using System.Linq;
using Parsley;
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
            typeRegistry.TypeOf("Math").ShouldEqual(new NamedType("Math"));
        }
    }
}