using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class Function : TypedSyntaxTree, Binding
    {
        public Position Position { get; private set; }
        public NamedType ReturnType { get; private set; }
        public Name Name { get; private set; }
        public IEnumerable<Parameter> Parameters { get; private set; }
        public Expression Body { get; private set; }
        public DataType Type { get; private set; }

        public Function(Position position, NamedType returnType, Name name, IEnumerable<Parameter> parameters, Expression body)
            : this(position, returnType, name, parameters, body, null) { }

        private Function(Position position, NamedType returnType, Name name, IEnumerable<Parameter> parameters, Expression body, DataType type)
        {
            Position = position;
            ReturnType = returnType;
            Name = name;
            Parameters = parameters;
            Body = body;
            Type = type;
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        private DataType DeclaredType
        {
            get
            {
                DataType[] parameterTypes = Parameters.Select(p => p.Type).ToArray();

                return NamedType.Function(parameterTypes, ReturnType);
            }
        }

        public TypeChecked<Function> WithTypes(Environment environment)
        {
            //TODO: Factor suspicious similarity between this and Lambda.WithTypes(Environment);

            var localEnvironment = new Environment(environment);

            foreach (var parameter in Parameters)
                if (!localEnvironment.TryIncludeUniqueBinding(parameter))
                    return TypeChecked<Function>.DuplicateIdentifierError(parameter);

            TypeChecked<Expression> typeCheckedBody = Body.WithTypes(localEnvironment);

            if (typeCheckedBody.HasErrors)
                return TypeChecked<Function>.Failure(typeCheckedBody.Errors);

            Expression typedBody = typeCheckedBody.Syntax;

            var normalizer = environment.TypeNormalizer;
            var unifyErrors = normalizer.Unify(ReturnType, typedBody.Type);
            if (unifyErrors.Any())
                return TypeChecked<Function>.Failure(Position, unifyErrors);

            return TypeChecked<Function>.Success(new Function(Position, ReturnType, Name, Parameters, typedBody, DeclaredType));
        }

        string Binding.Identifier
        {
            get { return Name.Identifier; }
        }

        DataType Binding.Type
        {
            get { return DeclaredType; }
        }
    }
}