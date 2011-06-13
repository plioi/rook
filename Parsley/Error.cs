using System;

namespace Parsley
{
    public class Error<T> : Reply<T>
    {
        private readonly ErrorMessage errorMessage;

        public Error(Lexer unparsedTokens)
            : this(unparsedTokens, new ErrorMessage()) { }

        public Error(Lexer unparsedTokens, ErrorMessage errorMessage)
        {
            UnparsedTokens = unparsedTokens;

            this.errorMessage = errorMessage;
        }

        public T Value
        {
            get { throw new MemberAccessException(ToString()); }
        }

        public Lexer UnparsedTokens { get; private set; }

        public bool Success { get { return false; } }

        public ErrorMessageList ErrorMessages
        {
            get { return ErrorMessageList.Empty.With(errorMessage); }
        }

        public Reply<U> ParseRest<U>(Func<T, Parser<U>> constructNextParser)
        {
            return new Error<U>(UnparsedTokens, errorMessage);
        }

        public override string ToString()
        {
            Position position = UnparsedTokens.Position;
            return String.Format("({0}, {1}): {2}", position.Line, position.Column, ErrorMessages);
        }
    }
}