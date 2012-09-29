using System.Collections.Generic;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class Class : TypedSyntaxTree, Binding
    {
        public Position Position { get; private set; }
        public Name Name { get; private set; }
        public Vector<Function> Methods { get; private set; }
        public DataType Type { get; private set; }

        public Class(Position position, Name name, IEnumerable<Function> methods)
            : this(position, name, methods.ToVector(), UnknownType.Instance) { }

        public Class(Position position, Name name, Vector<Function> methods, DataType type)
        {
            Position = position;
            Name = name;
            Methods = methods;
            Type = type;
        }

        public Class WithType(DataType type)
        {
            return new Class(Position, Name, Methods, type);
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public Class WithTypes(TypeChecker visitor, Scope scope)
        {
            return visitor.TypeCheck(this, scope);
        }

        string Binding.Identifier
        {
            get { return Name.Identifier; }
        }
    }
}