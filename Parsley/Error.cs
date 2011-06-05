using System;

namespace Parsley
{
    public sealed class Error<T> : Parsed<T>
    {
        private string expectation { get; set; }

        public Error(Lexer unparsedTokens, string expectation = null)
        {
            UnparsedTokens = unparsedTokens;
            this.expectation = expectation;
        }

        public T Value
        {
            get { throw new MemberAccessException(ToString()); }
        }

        public Lexer UnparsedTokens { get; private set; }

        public bool IsError { get { return true; } }

        public string Message
        {
            get { return expectation == null ? "Parse error." : expectation + " expected"; }
        }

        public Parsed<U> ParseRest<U>(Func<T, Parser<U>> constructNextParser)
        {
            return new Error<U>(UnparsedTokens, expectation);
        }

        public override string ToString()
        {
            Position position = UnparsedTokens.Position;
            return String.Format("({0}, {1}): {2}", position.Line, position.Column, Message);
        }
    }
}