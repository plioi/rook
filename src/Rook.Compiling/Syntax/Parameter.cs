using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class Parameter : TypedSyntaxTree, Binding
    {
        public Position Position { get; private set; }
        public TypeName DeclaredTypeName { get; private set; }
        public string Identifier { get; private set; }
        public DataType Type { get; private set; }

        public Parameter(Position position, string identifier)
            : this(position, TypeName.Empty, identifier, UnknownType.Instance) { }

        public Parameter(Position position, TypeName declaredTypeName, string identifier)
            : this(position, declaredTypeName, identifier, UnknownType.Instance) { }

        private Parameter(Position position, TypeName declaredTypeName, string identifier, DataType type)
        {
            Position = position;
            DeclaredTypeName = declaredTypeName;
            Identifier = identifier;
            Type = type;
        }

        public Parameter WithType(DataType type)
        {
            return new Parameter(Position, DeclaredTypeName, Identifier, type);
        }

        public bool IsImplicitlyTyped
        {
            get { return DeclaredTypeName == TypeName.Empty; }
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}