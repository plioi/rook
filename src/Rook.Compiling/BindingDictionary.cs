using System.Collections.Generic;
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

        public bool TryIncludeUniqueBinding(string identifier, DataType type)
        {
            if (Contains(identifier))
                return false;

            bindings[identifier] = type;
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