using System;

namespace Parsley
{
    public sealed class Error<T> : Parsed<T>
    {
        public Error(Text unparsedText)
            : this(unparsedText, null)
        {
        }

        public Error(Text unparsedText, string expectation)
        {
            UnparsedText = unparsedText;
            Expectation = expectation;
        }

        public T Value
        {
            get { throw new MemberAccessException(ToString()); }
        }

        public Text UnparsedText { get; private set; }

        public bool IsError { get { return true; } }

        public string Message
        {
            get { return Expectation == null ? "Parse error." : Expectation + " expected"; }
        }

        public string Expectation { get; private set; }

        public Parsed<U> ParseRest<U>(Func<T, Parser<U>> constructNextParser)
        {
            return new Error<U>(UnparsedText, Expectation);
        }

        public override string ToString()
        {
            Position position = UnparsedText.Position;
            return String.Format("({0}, {1}): {2}", position.Line, position.Column, Message);
        }
    }
}