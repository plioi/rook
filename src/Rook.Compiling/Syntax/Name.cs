using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class Name : Expression
    {
        public Position Position { get; private set; }
        public string Identifier { get; private set; }
        public DataType Type { get; private set; }

        public Name(Position position, string identifier)
            : this (position, identifier, null) { }

        private Name(Position position, string identifier, DataType type)
        {
            Position = position;
            Identifier = identifier;
            Type = type;
        }

        public TypeChecked<Expression> WithTypes(Scope scope)
        {
            DataType type;

            //TODO: We should probably normalize 'type' before freshening its variables.

            if (scope.TryGet(Identifier, out type))
                return TypeChecked<Expression>.Success(new Name(Position, Identifier, FreshenGenericTypeVariables(scope, type)));

            return TypeChecked<Expression>.UndefinedIdentifierError(Position, Identifier);
        }

        private static DataType FreshenGenericTypeVariables(Scope scope, DataType type)
        {
            var substitutions = new Dictionary<TypeVariable, DataType>();
            var genericTypeVariables = type.FindTypeVariables().Where(scope.IsGeneric);
            foreach (var genericTypeVariable in genericTypeVariables)
                substitutions[genericTypeVariable] = scope.CreateTypeVariable();

            return type.ReplaceTypeVariables(substitutions);
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}