using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class New : Expression
    {
        public Position Position { get; private set; }
        public Name TypeName { get; private set; }
        public DataType Type { get; private set; }

        public New(Position position, Name typeName)
            : this(position, typeName, UnknownType.Instance) { }

        public New(Position position, Name typeName, DataType type)
        {
            Position = position;
            TypeName = typeName;
            Type = type;
        }

        public Expression WithTypes(TypeChecker visitor, Scope scope)
        {
            return visitor.TypeCheck(this, scope);
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}