using System;
using System.Linq;
using Parsley;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class TypeChecked<T> where T : SyntaxTree
    {
        public T Syntax { get; private set; }
        public Vector<CompilerError> Errors { get; private set;}
        public bool HasErrors { get { return Errors.Any(); } }

        private TypeChecked(T syntax, Vector<CompilerError> errors)
        {
            Syntax = syntax;
            Errors = errors;
        }

        public static TypeChecked<T> Success(T syntax)
        {
            return new TypeChecked<T>(syntax, Enumerable.Empty<CompilerError>().ToVector());
        }

        private static TypeChecked<T> Failure(CompilerError error)
        {
            return new TypeChecked<T>(default(T), new[] { error }.ToVector());
        }

        public static TypeChecked<T> Failure(Vector<CompilerError> errors)
        {
            return new TypeChecked<T>(default(T), errors);
        }

        public static TypeChecked<T> InvalidConstantError(Position position, string literal)
        {
            return Failure(new CompilerError(position, "Invalid constant: " + literal));
        }

        public static TypeChecked<T> DuplicateIdentifierError(Binding duplicate)
        {
            return Failure(new CompilerError(duplicate.Position, "Duplicate identifier: " + duplicate.Identifier));
        }

        public static TypeChecked<T> UndefinedIdentifierError(Position position, string identifier)
        {
            return Failure(new CompilerError(position, "Reference to undefined identifier: " + identifier));
        }

        public static TypeChecked<T> ObjectNotCallableError(Position position)
        {
            return Failure(new CompilerError(position, "Attempted to call a noncallable object."));
        }

        public static TypeChecked<T> TypeNameExpectedForConstructionError(Position position, Name invalidTypeName)
        {
            return Failure(new CompilerError(position, String.Format("Cannot construct '{0}' because it is not a type.", invalidTypeName.Identifier)));
        }
    }
}