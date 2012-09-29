using System.Collections.Generic;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class Function : TypedSyntaxTree, Binding
    {
        public Position Position { get; private set; }
        public TypeName ReturnTypeName { get; private set; }
        public Name Name { get; private set; }
        public Vector<Parameter> Parameters { get; private set; }
        public Expression Body { get; private set; }
        public DataType Type { get; private set; }

        public Function(Position position, TypeName returnTypeName, Name name, IEnumerable<Parameter> parameters, Expression body)
            : this(position, returnTypeName, name, parameters.ToVector(), body, UnknownType.Instance) { }

        public Function(Position position, TypeName returnTypeName, Name name, Vector<Parameter> parameters, Expression body, DataType type)
        {
            Position = position;
            ReturnTypeName = returnTypeName;
            Name = name;
            Parameters = parameters;
            Body = body;
            Type = type;
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public Function WithTypes(TypeChecker visitor, Scope scope)
        {
            return visitor.TypeCheck(this, scope);
        }

        string Binding.Identifier
        {
            get { return Name.Identifier; }
        }
    }
}