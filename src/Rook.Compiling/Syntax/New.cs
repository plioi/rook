using System.Linq;
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

        public TypeChecked<Expression> WithTypes(Scope scope)
        {
            var typeCheckedTypeName = TypeName.WithTypes(scope);

            if (typeCheckedTypeName.HasErrors)
                return typeCheckedTypeName;

            var typedTypeName = (Name)typeCheckedTypeName.Syntax;

            var constructorType = typedTypeName.Type as NamedType;

            if (constructorType == null || constructorType.Name != "Rook.Core.Constructor")
                return TypeChecked<Expression>.TypeNameExpectedForConstructionError(TypeName.Position, TypeName);

            var constructedType = constructorType.InnerTypes.Last();

            return TypeChecked<Expression>.Success(new New(Position, typedTypeName, constructedType));
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}