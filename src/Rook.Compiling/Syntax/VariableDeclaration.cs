using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class VariableDeclaration : TypedSyntaxTree, Binding
    {
        public Position Position { get; private set; }
        public TypeName DeclaredTypeName { get; private set; }
        public string Identifier { get; private set; }
        public Expression Value { get; private set; }
        public DataType Type { get; private set; }

        public VariableDeclaration(Position position, string identifier, Expression value)
            : this(position, TypeName.Empty, identifier, value, UnknownType.Instance) { }

        public VariableDeclaration(Position position, TypeName declaredTypeName, string identifier, Expression value)
            : this(position, declaredTypeName, identifier, value, UnknownType.Instance) { }

        public VariableDeclaration(Position position, TypeName declaredTypeName, string identifier, Expression value, DataType type)
        {
            Position = position;
            DeclaredTypeName = declaredTypeName;
            Identifier = identifier;
            Value = value;
            Type = type;
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