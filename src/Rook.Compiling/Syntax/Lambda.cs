using System.Collections.Generic;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class Lambda : Expression
    {
        public Position Position { get; private set; }
        public Vector<Parameter> Parameters { get; private set; }
        public Expression Body { get; private set; }
        public DataType Type { get; private set; }

        public Lambda(Position position, IEnumerable<Parameter> parameters, Expression body)
        : this(position, parameters.ToVector(), body, null) { }

        public Lambda(Position position, Vector<Parameter> parameters, Expression body, DataType type)
        {
            Position = position;
            Parameters = parameters;
            Body = body;
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