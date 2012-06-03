﻿using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class Null : Expression
    {
        public Position Position { get; private set; }
        public DataType Type { get; private set; }

        public Null(Position position) 
            : this(position, null) { }

        private Null(Position position, DataType type)
        {
            Position = position;
            Type = type;
        }

        public TypeChecked<Expression> WithTypes(Scope scope, TypeUnifier unifier)
        {
            var result = new Null(Position, NamedType.Nullable(scope.CreateTypeVariable()));
            return TypeChecked<Expression>.Success(result);
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}