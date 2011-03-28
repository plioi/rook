using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public sealed class Block : Expression
    {
        public Position Position { get; private set; }
        public IEnumerable<VariableDeclaration> VariableDeclarations { get; private set; }
        public IEnumerable<Expression> InnerExpressions { get; private set; }
        public DataType Type { get; private set; }

        public Block(Position position, IEnumerable<VariableDeclaration> variableDeclarations, IEnumerable<Expression> innerExpressions)
            : this(position, variableDeclarations, innerExpressions, null) { }

        public Block(Position position, IEnumerable<VariableDeclaration> variableDeclarations, IEnumerable<Expression> innerExpressions, DataType type)
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

            Environment localEnvironment = new Environment(environment);

            List<VariableDeclaration> typedVariableDeclarations = new List<VariableDeclaration>();
            foreach (var variable in VariableDeclarations)
            {
                TypeChecked<Expression> typeCheckedValue = variable.Value.WithTypes(localEnvironment);

                if (typeCheckedValue.HasErrors)
                    return TypeChecked<Expression>.Failure(typeCheckedValue.Errors);

                Expression typedValue = typeCheckedValue.Syntax;
                
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
                var unifyErrors = normalizer.Unify(binding.Type, typedValue.Type);
                if (unifyErrors.Any())
                    return TypeChecked<Expression>.Failure(variable.Value.Position, unifyErrors);
            }

            IEnumerable<TypeChecked<Expression>> typeCheckedInnerExpressions = InnerExpressions.WithTypes(localEnvironment);

            var errors = typeCheckedInnerExpressions.Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            IEnumerable<Expression> typedInnerExpressions = typeCheckedInnerExpressions.Expressions();

            DataType blockType = typedInnerExpressions.Last().Type;

            return TypeChecked<Expression>.Success(new Block(Position, typedVariableDeclarations, typedInnerExpressions, blockType));
        }
    }
}