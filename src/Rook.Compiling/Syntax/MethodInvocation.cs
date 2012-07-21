using System.Collections.Generic;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class MethodInvocation : Expression
    {
        public Position Position { get; private set; }
        public Expression Instance { get; private set; }
        public Name MethodName { get; private set; }
        public Vector<Expression> Arguments { get; private set; }
        public DataType Type { get; private set; }

        public MethodInvocation(Position position, Expression instance, Name methodName, IEnumerable<Expression> arguments)
            : this(position, instance, methodName, arguments.ToVector(), null) { }

        public MethodInvocation(Position position, Expression instance, Name methodName, Vector<Expression> arguments, DataType type)
        {
            Position = position;
            Instance = instance;
            MethodName = methodName;
            Arguments = arguments;
            Type = type;
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public Expression WithTypes(TypeChecker visitor, Scope scope)
        {
            return visitor.TypeCheck(this, scope);
        }
    }
}