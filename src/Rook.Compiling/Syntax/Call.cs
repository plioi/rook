﻿using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class Call : Expression
    {
        public Position Position { get; private set; }
        public Expression Callable { get; private set; }
        public Vector<Expression> Arguments { get; private set; }
        public bool IsOperator { get; private set; }
        public DataType Type { get; private set; }

        public Call(Position position, string symbol, Expression operand)
            : this(position, new Name(position, symbol), new[] { operand }.ToVector(), true, null) { }

        public Call(Position position, string symbol, Expression leftOperand, Expression rightOperand)
            : this(position, new Name(position, symbol), new[] { leftOperand, rightOperand }.ToVector(), true, null) { }

        public Call(Position position, Expression callable, IEnumerable<Expression> arguments)
            : this(position, callable, arguments.ToVector(), false, null) { }

        public Call(Position position, Expression callable, Vector<Expression> arguments, bool isOperator, DataType type)
        {
            Position = position;
            Callable = callable;
            Arguments = arguments;
            IsOperator = isOperator;
            Type = type;
        }



        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public TypeChecked<Expression> WithTypes(TypeChecker visitor, Scope scope, TypeUnifier unifier)
        {
            return visitor.TypeCheck(this, scope, unifier);
        }
    }
}