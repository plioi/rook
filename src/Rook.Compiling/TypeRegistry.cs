using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling
{
    public class TypeRegistry
    {
        private readonly IDictionary<NamedType, List<Binding>> typeMembers;

        public TypeRegistry()
        {
            typeMembers = new Dictionary<NamedType, List<Binding>>();
        }

        public void Register(Class @class)
        {
            var typeKey = new NamedType(@class.Name.Identifier);

            Register(typeKey, @class.Methods.Cast<Binding>().ToArray());
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
        public bool TryGetMembers(NamedType typeKey, out IEnumerable<Binding> memberBindings)
        {
            if (typeMembers.ContainsKey(typeKey))
            {
                memberBindings = typeMembers[typeKey];
                return true;
            }

            memberBindings = null;
            return false;
        }
    }
}