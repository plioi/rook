using System.Collections.Generic;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling
{
    public class TypeRegistry
    {
        private readonly IDictionary<string, NamedType> types;

        public TypeRegistry()
        {
            types = new Dictionary<string, NamedType>();
        }

        public void Add(Class @class)
        {
            types.Add(@class.Name.Identifier, new NamedType(@class));
        }

        public NamedType TypeOf(string name)
        {
            return types[name];
        }
    }
}