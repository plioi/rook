using System;
using System.Collections.Generic;
using System.Linq;

namespace Rook.Compiling.Types
{
    public class NamedType : DataType
    {
        #region Factory Methods

        public static NamedType Dynamic
        {
            get { return new NamedType("dynamic"); }
        }

        public static NamedType Void
        {
            get { return new NamedType("Rook.Core.Void"); }
        }

        public static NamedType Boolean
        {
            get { return new NamedType("System.Boolean"); }
        }

        public static NamedType String
        {
            get { return new NamedType("System.String"); }
        }

        public static NamedType Integer
        {
            get { return new NamedType("System.Int32"); }
        }

        public static NamedType Enumerable(DataType itemType)
        {
            return new NamedType("System.Collections.Generic.IEnumerable", itemType);
        }

        public static NamedType Vector(DataType itemType)
        {
            return new NamedType("Rook.Core.Collections.Vector", itemType);
        }

        public static NamedType Nullable(DataType itemType)
        {
            return new NamedType("Rook.Core.Nullable", itemType);
        }

        public static NamedType Function(IEnumerable<DataType> parameterTypes, DataType returnType)
        {
            return new NamedType("System.Func", Enumerate(parameterTypes, returnType).ToArray());
        }

        public static NamedType Function(DataType returnType)
        {
            return Function(new DataType[] {}, returnType);
        }

        public static NamedType Constructor(DataType constructedType)
        {
            return new NamedType("Rook.Core.Constructor", constructedType);
        }

        private static IEnumerable<DataType> Enumerate(IEnumerable<DataType> parameterTypes, DataType returnType)
        {
            foreach (var type in parameterTypes)
                yield return type;
            yield return returnType;
        }

        #endregion

        private readonly string name;
        private readonly IEnumerable<DataType> innerTypes;
        private readonly Lazy<string> fullName;

        public NamedType(string name, params DataType[] innerTypes)
        {
            this.name = name;
            this.innerTypes = innerTypes;
            fullName = new Lazy<string>(GetFullName);
        }

        public override string Name
        {
            get { return name; }
        }

        public override IEnumerable<DataType> InnerTypes
        {
            get { return innerTypes; }
        }

        public override bool Contains(TypeVariable typeVariable)
        {
            return innerTypes.Any(innerType => innerType.Contains(typeVariable));
        }

        public override IEnumerable<TypeVariable> FindTypeVariables()
        {
            return innerTypes.SelectMany(t => t.FindTypeVariables()).Distinct();
        }

        public override DataType ReplaceTypeVariables(IDictionary<TypeVariable, DataType> substitutions)
        {
            return new NamedType(name, innerTypes.Select(t => t.ReplaceTypeVariables(substitutions)).ToArray());
        }

        public override string ToString()
        {
            return fullName.Value;
        }

        private string GetFullName()
        {
            if (innerTypes.Any())
                return System.String.Format("{0}<{1}>", CleanedName(name), System.String.Join(", ", innerTypes));
            
            return CleanedName(name);
        }

        private static string CleanedName(string name)
        {
            return name
                .Replace("System.Boolean", "bool")
                .Replace("System.Int32", "int")
                .Replace("System.String", "string");
        }
    }
}