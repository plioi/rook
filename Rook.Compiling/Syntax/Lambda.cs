using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public sealed class Lambda : Expression
    {
        public Position Position { get; private set; }
        public IEnumerable<Parameter> Parameters { get; private set; }
        public Expression Body { get; private set; }
        public DataType Type { get; private set; }

        public Lambda(Position position, IEnumerable<Parameter> parameters, Expression body)
        : this(position, parameters, body, null) { }

        private Lambda(Position position, IEnumerable<Parameter> parameters, Expression body, DataType type)
        {
            Position = position;
            Parameters = parameters;
            Body = body;
            Type = type;
        }

        public TypeChecked<Expression> WithTypes(Environment environment)
        {
            //TODO: Factor suspicious similarity between this and Function.WithTypes(Environment);
            //TODO: Factor suspicious similarity between this and Block.WithTypes(Environment);

            var localEnvironment = new Environment(environment);

            var typedParameters = ReplaceImplicitTypesWithNewNonGenericTypeVariables(Parameters, localEnvironment);

            foreach (var parameter in typedParameters)
                if (!localEnvironment.TryIncludeUniqueBinding(parameter))
                    return TypeChecked<Expression>.DuplicateIdentifierError(parameter);

            TypeChecked<Expression> typeCheckedBody = Body.WithTypes(localEnvironment);

            if (typeCheckedBody.HasErrors)
                return TypeChecked<Expression>.Failure(typeCheckedBody.Errors);

            Expression typedBody = typeCheckedBody.Syntax;

            var normalizedParameters = NormalizeTypes(typedParameters, localEnvironment);
            //TODO: Determine whether I should also normalize typedBody.Type for the return below.

            DataType[] parameterTypes = normalizedParameters.Select(p => p.Type).ToArray();

            return TypeChecked<Expression>.Success(new Lambda(Position, normalizedParameters, typedBody, NamedType.Function(parameterTypes, typedBody.Type)));
        }

        private static IEnumerable<Parameter> ReplaceImplicitTypesWithNewNonGenericTypeVariables(IEnumerable<Parameter> parameters, Environment localEnvironment)
        {
            var decoratedParameters = new List<Parameter>();
            var typeVariables = new List<TypeVariable>();

            foreach (var parameter in parameters)
            {
                if (parameter.IsImplicitlyTyped())
                {
                    TypeVariable typeVariable = localEnvironment.CreateTypeVariable();
                    typeVariables.Add(typeVariable);
                    decoratedParameters.Add(new Parameter(parameter.Position, typeVariable, parameter.Identifier));
                }
                else
                {
                    decoratedParameters.Add(parameter);
                }
            }

            localEnvironment.TreatAsNonGeneric(typeVariables);

            return decoratedParameters;
        }

        private static IEnumerable<Parameter> NormalizeTypes(IEnumerable<Parameter> typedParameters, Environment localEnvironment)
        {
            return typedParameters.Select(p => new Parameter(p.Position, localEnvironment.TypeNormalizer.Normalize(p.Type), p.Identifier));
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}