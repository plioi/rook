using System.Collections.Generic;
using System.Linq;

namespace Rook.Compiling.Types
{
    public class UnknownType : DataType
    {
        public static readonly UnknownType Instance = new UnknownType();

        private UnknownType() { }

        public override string Name
        {
            get { return "?"; }
        }

        public override IEnumerable<DataType> GenericArguments
        {
            get { return Enumerable.Empty<DataType>(); }
        }

        public override bool IsGeneric
        {
            get { return false; }
        }

        public override bool Contains(TypeVariable typeVariable)
        {
            return false;
        }

        public override IEnumerable<TypeVariable> FindTypeVariables()
        {
            return Enumerable.Empty<TypeVariable>();
        }

        public override DataType ReplaceTypeVariables(IDictionary<TypeVariable, DataType> substitutions)
        {
            return this;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}