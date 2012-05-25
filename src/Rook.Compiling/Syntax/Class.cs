using System.Collections.Generic;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class Class : TypedSyntaxTree, Binding
    {
        public Position Position { get; private set; }
        public Name Name { get; private set; }
        public IEnumerable<Function> Methods { get; private set; }
        public DataType Type { get; private set; }

        public Class(Position position, Name name, IEnumerable<Function> methods)
            : this(position, name, methods, ConstructorFunctionType(name)) { }

        private Class(Position position, Name name, IEnumerable<Function> methods, DataType type)
        {
            Position = position;
            Name = name;
            Methods = methods;
            Type = type;
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public TypeChecked<Class> WithTypes(Environment environment)
        {
            return TypeChecked<Class>.Success(this);
        }

        private static NamedType ConstructorFunctionType(Name name)
        {
            return NamedType.Constructor(new NamedType(name.Identifier));
        }

        string Binding.Identifier
        {
            get { return Name.Identifier; }
        }
    }
}