using System.Collections.Generic;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling
{
    public class TypeRegistry
    {
        private readonly IDictionary<string, DataType> types;

        public TypeRegistry()
        {
            types = new Dictionary<string, DataType>();
        }

        public void Add(Class @class)
        {
            types.Add(@class.Name.Identifier, new NamedType(@class));
        }

        public DataType TypeOf(string name)
        {
            return types[name];
        }
    }
}