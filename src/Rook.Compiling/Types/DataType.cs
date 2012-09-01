using System.Collections.Generic;
using System.Linq;
using Rook.Core;

namespace Rook.Compiling.Types
{
    public abstract class DataType : Value<DataType>
    {
        public abstract string Name { get; }

        public abstract IEnumerable<DataType> InnerTypes { get; }

        public abstract bool IsGeneric { get; }

        public abstract bool Contains(TypeVariable typeVariable);

        public abstract IEnumerable<TypeVariable> FindTypeVariables();
        
        public abstract DataType ReplaceTypeVariables(IDictionary<TypeVariable, DataType> substitutions);

        protected override object[] ImmutableFields()
        {
            //TODO: Ideally, we'd return new object[] { Name, InnerTypes },
            //      but the IEnumerable<T> being compared would probably not
            //      implement value equality.  Change InnerTypes to be a 
            //      collection type with value equality semantics.
            return new object[] {ToString()};
        }

        public DataType FreshenGenericTypeVariables()
        {
            var genericTypeVariables = FindTypeVariables().Where(x => x.IsGeneric);

            var substitutions = new Dictionary<TypeVariable, DataType>();
            foreach (var genericTypeVariable in genericTypeVariables)
                substitutions[genericTypeVariable] = TypeVariable.CreateGeneric();

            return ReplaceTypeVariables(substitutions);
        }
    }
}