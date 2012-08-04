using System.Collections.Generic;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling
{
    public class BindingDictionary
    {
        private readonly IDictionary<string, DataType> bindings;

        public BindingDictionary()
        {
            bindings = new Dictionary<string, DataType>();
        }

        public DataType this[string identifier]
        {
            set { bindings[identifier] = value; }
        }

        public bool TryIncludeUniqueBinding(Binding binding)
        {
            if (Contains(binding.Identifier))
                return false;

            bindings[binding.Identifier] = binding.Type;
            return true;
        }

        public bool TryGet(string identifier, out DataType type)
        {
            if (Contains(identifier))
            {
                type = bindings[identifier];
                return true;
            }

            type = null;
            return false;
        }

        public bool Contains(string identifier)
        {
            return bindings.ContainsKey(identifier);
        }
    }
}