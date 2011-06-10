using System.Collections.Generic;
using System.Linq;

namespace Rook.Compiling.Types
{
    public class TypeVariable : DataType
    {
        private readonly int name;

        public TypeVariable(int name)
        {
            this.name = name;
        }

        public override string Name
        {
            get { return name.ToString(); }
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

        public override bool Equals(object v)
        {
            return v != null && Equals(v as TypeVariable);
        }

        public bool Equals(TypeVariable v)
        {
            return v != null && name == v.name;
        }

        public static bool operator ==(TypeVariable a, TypeVariable b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.name == b.name;
        }

        public static bool operator !=(TypeVariable a, TypeVariable b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}