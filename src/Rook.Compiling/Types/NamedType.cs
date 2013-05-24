using System;
using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Syntax;
using Rook.Core;
using Rook.Core.Collections;
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
            return new NamedType(typeof(IEnumerable<>)).MakeGenericType(itemType);
        }

        public static NamedType Vector(DataType itemType)
        {
            return new NamedType(typeof(Vector<>)).MakeGenericType(itemType);
        }

        public static NamedType Nullable(DataType type)
        {
            return new NamedType(typeof(Core.Nullable<>)).MakeGenericType(type);
        }

        public static NamedType Constructor(DataType constructedType)
        {
            return new NamedType(typeof(Constructor<>)).MakeGenericType(constructedType);
        }

        public static NamedType Function(IEnumerable<DataType> parameterTypes, DataType returnType)
        {
            return new NamedType("System.Func", Enumerate(parameterTypes, returnType).ToArray());
        }

        public static NamedType Function(DataType returnType)
        {
            return Function(new DataType[] {}, returnType);
        }

        private static IEnumerable<DataType> Enumerate(IEnumerable<DataType> parameterTypes, DataType returnType)
        {
            foreach (var type in parameterTypes)
                yield return type;
            yield return returnType;
        }

        private readonly string name;
        private readonly Vector<DataType> genericArguments;
        private readonly Lazy<string> fullName;
        private readonly bool isGenericTypeDefinition;

        [Obsolete]
        public NamedType(string name, params DataType[] genericArguments)
        {
            this.name = name;
            this.genericArguments = genericArguments.ToVector();
            isGenericTypeDefinition = false;
            fullName = new Lazy<string>(GetFullName);
        }

        public NamedType(Class @class)
            : this(@class.Name.Identifier)
        {
        }

        public NamedType(Type type)
        {
            if (type.IsGenericParameter)
                throw new ArgumentException("NamedType cannot be constructed for generic parameter: " + type);

            var genericArguments = type.GetGenericArguments();

            name = type.QualifiedName();

            isGenericTypeDefinition = type.IsGenericTypeDefinition;

            this.genericArguments = isGenericTypeDefinition
                ? genericArguments.Select(x => (DataType)TypeVariable.CreateGeneric()).ToVector()
                : genericArguments.Select(x => (DataType)new NamedType(x)).ToVector();

            fullName = new Lazy<string>(GetFullName);
        }

        public NamedType MakeGenericType(params DataType[] typeArguments)
        {
            if (!IsGenericTypeDefinition)
                throw new InvalidOperationException(this + " is not a generic type definition, so it cannot be used to make generic types.");

            if (typeArguments.Length != genericArguments.Count)
                throw new ArgumentException("Invalid number of generic type arguments.");

            return new NamedType(name, typeArguments);
        }

        public override string Name
        {
            get { return name; }
        }

        public override Vector<DataType> GenericArguments
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
            if (!IsGeneric)
                return this;

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