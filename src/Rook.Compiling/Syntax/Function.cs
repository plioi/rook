using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class Function : TypedSyntaxTree, Binding
    {
        public Position Position { get; private set; }
        public NamedType ReturnType { get; private set; }
        public Name Name { get; private set; }
        public Vector<Parameter> Parameters { get; private set; }
        public Expression Body { get; private set; }
        public DataType Type { get; private set; }

        public Function(Position position, NamedType returnType, Name name, IEnumerable<Parameter> parameters, Expression body)
            : this(position, returnType, name, parameters.ToVector(), body, null) { }

        private Function(Position position, NamedType returnType, Name name, Vector<Parameter> parameters, Expression body, DataType type)
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
                var parameterTypes = Parameters.Select(p => p.Type).ToArray();

                return NamedType.Function(parameterTypes, ReturnType);
            }
        }

        public TypeChecked<Function> WithTypes(Scope scope, TypeUnifier unifier)
        {
            var localScope = new Scope(scope);

            foreach (var parameter in Parameters)
                if (!localScope.TryIncludeUniqueBinding(parameter))
                    return TypeChecked<Function>.DuplicateIdentifierError(parameter);

            var typeCheckedBody = Body.WithTypes(localScope, unifier);
            if (typeCheckedBody.HasErrors)
                return TypeChecked<Function>.Failure(typeCheckedBody.Errors);

            var typedBody = typeCheckedBody.Syntax;
            var unifyErrors = unifier.Unify(ReturnType, typedBody);
            if (unifyErrors.Count > 0)
                return TypeChecked<Function>.Failure(unifyErrors);

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