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

        public Function(Position position, NamedType returnType, Name name, Vector<Parameter> parameters, Expression body, DataType type)
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

        public DataType DeclaredType
        {
            get
            {
                var parameterTypes = Parameters.Select(p => p.Type).ToArray();

                return NamedType.Function(parameterTypes, ReturnType);
            }
        }

        public Function WithTypes(TypeChecker visitor, Scope scope)
        {
            return visitor.TypeCheck(this, scope);
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