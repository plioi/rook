using System;
using System.Collections.Generic;
using System.Linq;

namespace Rook.Compiling.Types
{
    public class NamedType : DataType
    {
        #region Factory Methods

        private static readonly IDictionary<string, NamedType> registeredTypes = new Dictionary<string, NamedType>();

        public static NamedType Dynamic
        {
            get { return Create("dynamic"); }
        }

        public static NamedType Void
        {
            get { return Create("Rook.Core.Void"); }
        }

        public static NamedType Boolean
        {
            get { return Create("System.Boolean"); }
        }

        public static NamedType Integer
        {
            get { return Create("System.Int32"); }
        }

        public static NamedType Enumerable(DataType itemType)
        {
            return Create("System.Collections.Generic.IEnumerable", itemType);
        }

        public static NamedType Vector(DataType itemType)
        {
            return Create("Rook.Core.Collections.Vector", itemType);
        }

        public static NamedType Nullable(DataType itemType)
        {
            return Create("Rook.Core.Nullable", itemType);
        }

        public static NamedType Function(IEnumerable<DataType> parameterTypes, DataType returnType)
        {
            return Create("System.Func", Enumerate(parameterTypes, returnType).ToArray());
        }

        public static NamedType Function(DataType returnType)
        {
            return Function(new DataType[] {}, returnType);
        }

        public static NamedType Create(string name, params DataType[] innerTypes)
        {
            return RegisteredType(new NamedType(name, innerTypes));
        }

        private static IEnumerable<DataType> Enumerate(IEnumerable<DataType> parameterTypes, DataType returnType)
        {
            foreach (var type in parameterTypes)
                yield return type;
            yield return returnType;
        }

        private static NamedType RegisteredType(NamedType type)
        {
            string name = type.ToString();

            if (!registeredTypes.ContainsKey(name))
                registeredTypes[name] = type;

            return registeredTypes[name];
        }

        #endregion

        private readonly string name;
        private readonly IEnumerable<DataType> innerTypes;

        private NamedType(string name, IEnumerable<DataType> innerTypes)
        {
            this.name = name;
            this.innerTypes = innerTypes;
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
            return Create(name, innerTypes.Select(t => t.ReplaceTypeVariables(substitutions)).ToArray());
        }

        public override string ToString()
        {
            if (innerTypes.Any())
                return String.Format("{0}<{1}>", CleanedName(name), String.Join(", ", innerTypes));
            
            return CleanedName(name);
        }

        private static string CleanedName(string name)
        {
            return name
                .Replace("System.Boolean", "bool")
                .Replace("System.Int32", "int");
        }
    }
}