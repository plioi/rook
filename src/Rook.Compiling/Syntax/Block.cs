using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class Block : Expression
    {
        public Position Position { get; private set; }
        public IEnumerable<VariableDeclaration> VariableDeclarations { get; private set; }
        public IEnumerable<Expression> InnerExpressions { get; private set; }
        public DataType Type { get; private set; }

        public Block(Position position, IEnumerable<VariableDeclaration> variableDeclarations, IEnumerable<Expression> innerExpressions)
            : this(position, variableDeclarations, innerExpressions, null) { }

        private Block(Position position, IEnumerable<VariableDeclaration> variableDeclarations, IEnumerable<Expression> innerExpressions, DataType type)
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

        public TypeChecked<Expression> WithTypes(Environment environment)
        {
            //TODO: Factor suspicious similarity between this and Lambda.WithTypes(Environment);

            var localEnvironment = new Environment(environment);

            var typedVariableDeclarations = new List<VariableDeclaration>();
            foreach (var variable in VariableDeclarations)
            {
                var typeCheckedValue = variable.Value.WithTypes(localEnvironment);

                if (typeCheckedValue.HasErrors)
                    return TypeChecked<Expression>.Failure(typeCheckedValue.Errors);

                var typedValue = typeCheckedValue.Syntax;
                
                Binding binding = variable;
                if (variable.IsImplicitlyTyped())
                    binding = new VariableDeclaration(variable.Position, /*Replaces implicit type.*/ typedValue.Type, variable.Identifier, variable.Value);

                if (!localEnvironment.TryIncludeUniqueBinding(binding))
                    return TypeChecked<Expression>.DuplicateIdentifierError(binding);
                
                typedVariableDeclarations.Add(new VariableDeclaration(variable.Position,
                                                                      binding.Type,
                                                                      variable.Identifier,
                                                                      typedValue));

                var normalizer = environment.TypeNormalizer;
                var unifyErrors = normalizer.Unify(binding.Type, typedValue.Type).ToArray();
                if (unifyErrors.Any())
                    return TypeChecked<Expression>.Failure(variable.Value.Position, unifyErrors);
            }

            var typeCheckedInnerExpressions = InnerExpressions.WithTypes(localEnvironment);

            var errors = typeCheckedInnerExpressions.Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            var typedInnerExpressions = typeCheckedInnerExpressions.Expressions();

            var blockType = typedInnerExpressions.Last().Type;

            return TypeChecked<Expression>.Success(new Block(Position, typedVariableDeclarations, typedInnerExpressions, blockType));
        }
    }
}