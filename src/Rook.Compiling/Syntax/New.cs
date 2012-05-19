using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class New : Expression
    {
        public Position Position { get; private set; }
        public Name TypeName { get; private set; }

        public New(Position position, Name typeName)
        {
            Position = position;
            TypeName = typeName;
        }

        public DataType Type
        {
            get { throw new System.NotImplementedException(); }
        }

        public TypeChecked<Expression> WithTypes(Environment environment)
        {
            throw new System.NotImplementedException();
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}