using System.Collections.Generic;
using System.Linq;

namespace Rook.Compiling.Types
{
    public class TypeVariable : DataType
    {
        private readonly int name;
        private readonly bool isGeneric;

        public TypeVariable(int name)
            : this(name, true)
        {
        }

        public TypeVariable(int name, bool isGeneric)
        {
            this.name = name;
            this.isGeneric = isGeneric;
        }

        public override string Name
        {
            get { return name.ToString(); }
        }

        public bool IsGeneric
        {
            get { return isGeneric; }
        }

        public override IEnumerable<DataType> InnerTypes
        {
            get { return Enumerable.Empty<DataType>(); }
        }

        public override bool Contains(TypeVariable typeVariable)
        {
            return typeVariable == this;
        }

        public override IEnumerable<TypeVariable> FindTypeVariables()
        {
            yield return this;
        }

        public override DataType ReplaceTypeVariables(IDictionary<TypeVariable, DataType> substitutions)
        {
            if (substitutions.ContainsKey(this))
                return substitutions[this];

            return this;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}