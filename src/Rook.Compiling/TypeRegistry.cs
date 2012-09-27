using System;
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
        }

        public void Add(Class @class)
        {
            types.Add(new TypeName(@class.Name.Identifier), new NamedType(@class));
        }

        public NamedType TypeOf(TypeName name)
        {
            if (!types.ContainsKey(name))
                types.Add(name, ReflectType(name));

            return types[name];
        }

        private NamedType ReflectType(TypeName name)
        {
            //TODO: This relies on Type.GetType(string), which will only work for types referenced by this test
            //      assembly and types found in Mscorlib.dll, as the strings passed to it will not be assembly-qualified.
            //      TypeRegistry will eventually need to be configured with the necessary Assemblies to search within.

            var type = Type.GetType(name.ToString());

            if (type == null)
                return null;

            return new NamedType(type);
        }
    }
}