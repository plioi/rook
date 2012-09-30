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
            var sample = typeRegistry.TypeOf(TypeName.Integer);
            sample.Name.ShouldEqual("System.Int32");
            sample.IsGeneric.ShouldBeFalse();
            sample.ToString().ShouldEqual("int");

            sample = typeRegistry.TypeOf(TypeName.Boolean);
            sample.Name.ShouldEqual("System.Boolean");
            sample.IsGeneric.ShouldBeFalse();
            sample.ToString().ShouldEqual("bool");

            sample = typeRegistry.TypeOf(TypeName.String);
            sample.Name.ShouldEqual("System.String");
            sample.IsGeneric.ShouldBeFalse();
            sample.ToString().ShouldEqual("string");

            sample = typeRegistry.TypeOf(TypeName.Void);
            sample.Name.ShouldEqual("Rook.Core.Void");
            sample.IsGeneric.ShouldBeFalse();
            sample.ToString().ShouldEqual("Rook.Core.Void");
        }

        public void ShouldGetTypesForRegisteredClasses()
        {
            var math = "class Math { int Square(int x) x*x; bool Zero(int x) x==0; }".ParseClass();

            typeRegistry.Add(math);
            typeRegistry.TypeOf(new TypeName("Math")).ShouldEqual(new NamedType(math));
        }
    }
}