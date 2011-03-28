using System.Collections.Generic;

namespace Rook.Compiling.Types
{
    public abstract class DataType
    {
        public abstract string Name { get; }

        public abstract IEnumerable<DataType> InnerTypes { get; }

        public abstract bool Contains(TypeVariable typeVariable);

        public abstract IEnumerable<TypeVariable> FindTypeVariables();
        
        public abstract DataType ReplaceTypeVariables(IDictionary<TypeVariable, DataType> substitutions);
    }
}