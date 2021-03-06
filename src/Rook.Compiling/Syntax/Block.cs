using System.Collections.Generic;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class Block : Expression
    {
        public Position Position { get; private set; }
        public Vector<VariableDeclaration> VariableDeclarations { get; private set; }
        public Vector<Expression> InnerExpressions { get; private set; }
        public DataType Type { get; private set; }

        public Block(Position position, IEnumerable<VariableDeclaration> variableDeclarations, IEnumerable<Expression> innerExpressions)
            : this(position, variableDeclarations.ToVector(), innerExpressions.ToVector(), UnknownType.Instance) { }

        public Block(Position position, Vector<VariableDeclaration> variableDeclarations, Vector<Expression> innerExpressions, DataType type)
        {
            Position = position;
            VariableDeclarations = variableDeclarations;
            InnerExpressions = innerExpressions;
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