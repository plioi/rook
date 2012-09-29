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

        public TypeMemberRegistry()
        {
            typeMembers = new Dictionary<NamedType, List<Binding>>();
        }

        public void Register(Class @class)
        {
            var typeKey = new NamedType(@class);
            var memberBindings = typeKey.Methods;

            Register(typeKey, memberBindings.ToArray());
        }

        //TODO: Deprecated.
        public void Register(NamedType typeKey, params Binding[] memberBindings)
        {
            if (!typeMembers.ContainsKey(typeKey))
                typeMembers[typeKey] = new List<Binding>();

            var typeMemberBindings = typeMembers[typeKey];
            typeMemberBindings.AddRange(memberBindings);
        }

        //TODO: Just return empty collection for unknown types?  Is it important to distinguish empty versus unknown?
        //TODO: Deprecated: instead, ask for a type by string name and generic args, then ask the resulting DataType for its members.
        public bool TryGetMembers(NamedType typeKey, out Vector<Binding> memberBindings)
        {
            if (typeMembers.ContainsKey(typeKey))
            {
                memberBindings = typeMembers[typeKey].ToVector();
                return true;
            }

            memberBindings = null;
            return false;
        }
    }
}