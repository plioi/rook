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

        private Call(Position position, Expression callable, Vector<Expression> arguments, bool isOperator, DataType type)
        {
            Position = position;
            Callable = callable;
            Arguments = arguments;
            IsOperator = isOperator;
            Type = type;
        }

        public TypeChecked<Expression> WithTypes(Environment environment)
        {
            TypeChecked<Expression> typeCheckedCallable = Callable.WithTypes(environment);
            var typeCheckedArguments = Arguments.WithTypes(environment);

            var errors = new[] {typeCheckedCallable}.Concat(typeCheckedArguments).ToVector().Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            Expression typedCallable = typeCheckedCallable.Syntax;
            var typedArguments = typeCheckedArguments.Expressions();

            DataType calleeType = typedCallable.Type;
            NamedType calleeFunctionType = calleeType as NamedType;

            if (calleeFunctionType == null || calleeFunctionType.Name != "System.Func")
                return TypeChecked<Expression>.ObjectNotCallableError(Position);

            DataType returnType = calleeType.InnerTypes.Last();
            var argumentTypes = typedArguments.Types();

            var normalizer = environment.TypeNormalizer;
            var unifyErrors = normalizer.Unify(calleeType, NamedType.Function(argumentTypes, returnType)).ToArray();
            if (unifyErrors.Any())
                return TypeChecked<Expression>.Failure(Position, unifyErrors);

            DataType callType = normalizer.Normalize(returnType);

            return TypeChecked<Expression>.Success(new Call(Position, typedCallable, typedArguments, IsOperator, callType));
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}