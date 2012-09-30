using System.Collections.Generic;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling
{
    public class TypeRegistry
    {
        private readonly IDictionary<TypeName, NamedType> types;

        public TypeRegistry()
        {
            types = new Dictionary<TypeName, NamedType>();

            RegisterCommonTypes();
        }

        private void RegisterCommonTypes()
        {
            types.Add(TypeName.Integer, NamedType.Integer);
            types.Add(TypeName.Boolean, NamedType.Boolean);
            types.Add(TypeName.String, NamedType.String);
            types.Add(TypeName.Void, NamedType.Void);
        }

        public void Add(Class @class)
        {
            types.Add(new TypeName(@class.Name.Identifier), new NamedType(@class));
        }

        public NamedType TypeOf(TypeName name)
        {
            if (!types.ContainsKey(name))
                return null;

            return types[name];
        }
    }
}