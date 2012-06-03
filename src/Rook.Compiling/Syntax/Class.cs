using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class Class : TypedSyntaxTree, Binding, TypeMemberBinding
    {
        public Position Position { get; private set; }
        public Name Name { get; private set; }
        public Vector<Function> Methods { get; private set; }
        public DataType Type { get; private set; }

        public Class(Position position, Name name, IEnumerable<Function> methods)
            : this(position, name, methods.ToVector(), ConstructorFunctionType(name)) { }

        private Class(Position position, Name name, Vector<Function> methods, DataType type)
        {
            Position = position;
            Name = name;
            Methods = methods;
            Type = type;
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public TypeChecked<Class> WithTypes(Scope scope, TypeUnifier unifier)
        {
            var localScope = new Scope(scope);

            foreach (var method in Methods)
                if (!localScope.TryIncludeUniqueBinding(method))
                    return TypeChecked<Class>.DuplicateIdentifierError(method);

            var typeCheckedMethods = Methods.WithTypes(localScope, unifier);

            var errors = typeCheckedMethods.Errors();
            if (errors.Any())
                return TypeChecked<Class>.Failure(errors);

            return TypeChecked<Class>.Success(new Class(Position, Name, typeCheckedMethods.Functions()));
        }

        private static NamedType ConstructorFunctionType(Name name)
        {
            return NamedType.Constructor(new NamedType(name.Identifier));
        }

        string Binding.Identifier
        {
            get { return Name.Identifier; }
        }

        DataType TypeMemberBinding.Type
        {
            get { return new NamedType(Name.Identifier); }
        }

        Vector<Binding> TypeMemberBinding.Members
        {
            get { return Methods.ToVector<Binding>(); }
        }
    }
}