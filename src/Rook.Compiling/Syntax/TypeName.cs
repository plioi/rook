using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Types;
using Rook.Core;

namespace Rook.Compiling.Syntax
{
    public class TypeName : Value<TypeName>
    {
        public static readonly TypeName Empty = new TypeName();
        public static readonly TypeName Boolean = new TypeName(typeof(bool).FullName);
        public static readonly TypeName String = new TypeName(typeof(string).FullName);
        public static readonly TypeName Integer = new TypeName(typeof(int).FullName);
        public static readonly TypeName Void = new TypeName(typeof(Void).FullName);

        public static TypeName Enumerable(TypeName itemTypeName)
        {
            return new TypeName(typeof(IEnumerable<>).QualifiedName(), itemTypeName);
        }

        private readonly string name;
        private readonly TypeName[] genericArguments;
        private readonly string fullName;

        private TypeName()
        {
            name = "";
            genericArguments = new TypeName[] {};
            fullName = "";
        }

        public TypeName(string name, params TypeName[] genericArguments)
        {
            this.name = name;
            this.genericArguments = genericArguments;

            fullName = genericArguments.Any()
                           ? System.String.Format("{0}<{1}>", name, System.String.Join(", ", (IEnumerable<TypeName>) genericArguments))
                           : name;
        }

        public string Name
        {
            get { return name; }
        }

        public IEnumerable<TypeName> GenericArguments
        {
            get { return genericArguments; }
        }

        protected override object[] ImmutableFields()
        {
            return new object[] { fullName };
        }

        public override string ToString()
        {
            return fullName;
        }
    }
}
