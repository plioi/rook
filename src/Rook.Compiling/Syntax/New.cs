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
            :this(position, typeName, null) { }

        public New(Position position, Name typeName, DataType type)
        {
            Position = position;
            TypeName = typeName;
            Type = type;
        }

        public TypeChecked<Expression> WithTypes(Environment environment)
        {
            var typeCheckedTypeName = TypeName.WithTypes(environment);

            if (typeCheckedTypeName.HasErrors)
                return typeCheckedTypeName;

            //TODO: It isn't enough to just have a known identifier.
            //      The identifier should be the name of a known class.
            //      See how Call type checks by expecting a Func type.
            //      New should check by expecting a class type.
            //      Consider changing the meaning of "the type of a class decaration".

            var typedTypeName = (Name)typeCheckedTypeName.Syntax;

            return TypeChecked<Expression>.Success(new New(Position, typedTypeName, new NamedType(TypeName.Identifier)));
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}