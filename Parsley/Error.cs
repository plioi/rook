using System;

namespace Parsley
{
    public class Error<T> : Reply<T>
    {
        private ErrorMessage errorMessage { get; set; }

        public Error(Lexer unparsedTokens, string expectation = null)
        {
            UnparsedTokens = unparsedTokens;

            errorMessage = new ErrorMessage(expectation);
        }

        public T Value
        {
            get { throw new MemberAccessException(ToString()); }
        }

        public Lexer UnparsedTokens { get; private set; }

        public bool Success { get { return false; } }

        public string Message
        {
            get { return errorMessage.ToString(); }
        }

        public Reply<U> ParseRest<U>(Func<T, Parser<U>> constructNextParser)
        {
            return new Error<U>(UnparsedTokens, errorMessage.Expectation);
        }

        public override string ToString()
        {
            Position position = UnparsedTokens.Position;
            return String.Format("({0}, {1}): {2}", position.Line, position.Column, Message);
        }
    }
}