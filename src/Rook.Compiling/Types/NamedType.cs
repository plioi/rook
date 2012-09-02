using System;
using System.Collections.Generic;
using System.Linq;
using Void = Rook.Core.Void;

namespace Rook.Compiling.Types
{
    public class NamedType : DataType
    {
        public static readonly NamedType Void = new NamedType(typeof(Void));
        public static readonly NamedType Boolean = new NamedType(typeof(bool));
        public static readonly NamedType String = new NamedType(typeof(string));
        public static readonly NamedType Integer = new NamedType(typeof(int));

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

        private readonly string name;
        private readonly DataType[] genericArguments;
        private readonly Lazy<string> fullName;
        private readonly bool isGenericTypeDefinition;

        public NamedType(string name, params DataType[] genericArguments)
        {
            this.name = name;
            this.genericArguments = genericArguments;
            isGenericTypeDefinition = false;
            fullName = new Lazy<string>(GetFullName);
        }

        public NamedType(Type type)
        {
            if (type.IsGenericParameter)
                throw new ArgumentException("NamedType cannot be constructed for generic parameter: " + type);

            var genericArguments = type.GetGenericArguments();

            name = type.Namespace + "." + type.Name.Replace("`" + genericArguments.Length, "");

            isGenericTypeDefinition = type.IsGenericTypeDefinition;

            this.genericArguments = isGenericTypeDefinition
                ? genericArguments.Select(x => (DataType)TypeVariable.CreateGeneric()).ToArray()
                : genericArguments.Select(x => (DataType)new NamedType(x)).ToArray();

            fullName = new Lazy<string>(GetFullName);
        }

        public override string Name
        {
            get { return name; }
        }

        public override IEnumerable<DataType> GenericArguments
        {
            get { return genericArguments; }
        }

        public override bool IsGeneric
        {
            get { return GenericArguments.Any(); }
        }

        public override bool IsGenericTypeDefinition
        {
            get { return isGenericTypeDefinition; }
        }

        public override bool Contains(TypeVariable typeVariable)
        {
            return genericArguments.Any(genericArgument => genericArgument.Contains(typeVariable));
        }

        public override IEnumerable<TypeVariable> FindTypeVariables()
        {
            return genericArguments.SelectMany(t => t.FindTypeVariables()).Distinct();
        }

        public override DataType ReplaceTypeVariables(IDictionary<TypeVariable, DataType> substitutions)
        {
            return new NamedType(name, genericArguments.Select(t => t.ReplaceTypeVariables(substitutions)).ToArray());
        }

        public override string ToString()
        {
            return fullName.Value;
        }

        private string GetFullName()
        {
            if (genericArguments.Any())
                return System.String.Format("{0}<{1}>", CleanedName, System.String.Join(", ", (IEnumerable<DataType>)genericArguments));
            
            return CleanedName;
        }

        private string CleanedName
        {
            get
            {
                return name
                    .Replace("System.Boolean", "bool")
                    .Replace("System.Int32", "int")
                    .Replace("System.String", "string");
            }
        }
    }
}