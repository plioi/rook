using System.Collections.Generic;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class VectorLiteral : Expression
    {
        public Position Position { get; private set; }
        public Vector<Expression> Items { get; private set; }
        public DataType Type { get; private set; }

        public VectorLiteral(Position position, IEnumerable<Expression> items)
            : this(position, items.ToVector(), null) { }

        public VectorLiteral(Position position, Vector<Expression> items, DataType type)
        {
            Position = position;
            Items = items;
            Type = type;
        }

        public TypeChecked<Expression> WithTypes(TypeChecker visitor, Scope scope, TypeUnifier unifier)
        {
            return visitor.TypeCheck(this, scope, unifier);
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}