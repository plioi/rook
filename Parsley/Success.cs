using System;

namespace Parsley
{
    public sealed class Success<T> : Parsed<T>
    {
        public Success(T value, Text unparsedText)
        {
            Value = value;
            UnparsedText = unparsedText;
        }

        public T Value { get; private set; }
        public Text UnparsedText { get; private set; }
        public bool IsError { get { return false; } }
        public string Message { get { return "Parse succeeded."; } }
        public Parsed<U> ParseRest<U>(Func<T, Parser<U>> constructNextParser)
        {
            return constructNextParser(Value)(UnparsedText);
        }
    }
}