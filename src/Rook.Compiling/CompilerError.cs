using System;
using Parsley;
using Rook.Compiling.Syntax;

namespace Rook.Compiling
{
    public class CompilerError
    {
        public CompilerError(Position position, string message)
        {
            Message = message;
            Position = position;
        }

        public Position Position { get; private set; }
        public string Message { get; private set; }

        public override string ToString()
        {
            return String.Format("{0}: {1}", Position, Message);
        }

        public static CompilerError InvalidConstant(Position position, string literal)
        {
            return new CompilerError(position, "Invalid constant: " + literal);
        }

        public static CompilerError DuplicateIdentifier(Position position, Binding duplicate)
        {
            return new CompilerError(position, "Duplicate identifier: " + duplicate.Identifier);
        }

        public static CompilerError UndefinedIdentifier(Name undefined)
        {
            return new CompilerError(undefined.Position, "Reference to undefined identifier: " + undefined.Identifier);
        }

        public static CompilerError ObjectNotCallable(Position position)
        {
            return new CompilerError(position, "Attempted to call a noncallable object.");
        }

        public static CompilerError AmbiguousMethodInvocation(Position position)
        {
            return new CompilerError(position, "Cannot invoke method against instance of unknown type.");
        }

        public static CompilerError TypeNameExpectedForConstruction(Position position, Name invalidTypeName)
        {
            return new CompilerError(position, String.Format("Cannot construct '{0}' because it is not a type.", invalidTypeName.Identifier));
        }
    }
}