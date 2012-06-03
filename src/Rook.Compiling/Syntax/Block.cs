using System.Collections.Generic;
using System.Linq;
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
            : this(position, variableDeclarations.ToVector(), innerExpressions.ToVector(), null) { }

        private Block(Position position, Vector<VariableDeclaration> variableDeclarations, Vector<Expression> innerExpressions, DataType type)
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

        public TypeChecked<Expression> WithTypes(Scope scope)
        {
            var localScope = new Scope(scope);

            var typedVariableDeclarations = new List<VariableDeclaration>();
            foreach (var variable in VariableDeclarations)
            {
                var typeCheckedValue = variable.Value.WithTypes(localScope);

                if (typeCheckedValue.HasErrors)
                    return typeCheckedValue;

                var typedValue = typeCheckedValue.Syntax;
                
                Binding binding = variable;
                if (variable.IsImplicitlyTyped())
                    binding = new VariableDeclaration(variable.Position, /*Replaces implicit type.*/ typedValue.Type, variable.Identifier, variable.Value);

                if (!localScope.TryIncludeUniqueBinding(binding))
                    return TypeChecked<Expression>.DuplicateIdentifierError(binding);
                
                typedVariableDeclarations.Add(new VariableDeclaration(variable.Position,
                                                                      binding.Type,
                                                                      variable.Identifier,
                                                                      typedValue));

                var unifyErrors = scope.TypeUnifier.Unify(binding.Type, typedValue);

                if (unifyErrors.Count > 0)
                    return TypeChecked<Expression>.Failure(unifyErrors);
            }

            var typeCheckedInnerExpressions = InnerExpressions.WithTypes(localScope);

            var errors = typeCheckedInnerExpressions.Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            var typedInnerExpressions = typeCheckedInnerExpressions.Expressions();

            var blockType = typedInnerExpressions.Last().Type;

            return TypeChecked<Expression>.Success(new Block(Position, typedVariableDeclarations.ToVector(), typedInnerExpressions, blockType));
        }
    }
}