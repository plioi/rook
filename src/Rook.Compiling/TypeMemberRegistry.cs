using System;
using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling
{
    [Obsolete]
    public class MethodBinding : Binding
    {
        public MethodBinding(string identifier, DataType type)
        {
            Identifier = identifier;
            Type = type;
        }

        public string Identifier { get; private set; }
        public DataType Type { get; private set; }
    }

    [Obsolete]
    public class TypeMemberRegistry
    {
        private readonly IDictionary<NamedType, List<Binding>> typeMembers;
        private readonly TypeRegistry typeRegistry;

        public TypeMemberRegistry(TypeRegistry typeRegistry)
        {
            this.typeRegistry = typeRegistry;
            typeMembers = new Dictionary<NamedType, List<Binding>>();
        }

        public void Register(Class @class)
        {
            var typeKey = new NamedType(@class, typeRegistry);
            var memberBindings = typeKey.Methods;

            Register(typeKey, memberBindings.ToArray());
        }

        public void Register(NamedType typeKey, params Binding[] memberBindings)
        {
            if (!typeMembers.ContainsKey(typeKey))
                typeMembers[typeKey] = new List<Binding>();

            typeMembers[typeKey].AddRange(memberBindings);
        }

        public Vector<Binding> TryGetMembers(NamedType typeKey)
        {
            if (typeMembers.ContainsKey(typeKey))
                return typeMembers[typeKey].ToVector();

            return null;
        }
    }
}