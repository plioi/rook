using System;

namespace Parsley
{
    public class Parsed<T> : Reply<T>
    {
        public Parsed(T value, Lexer unparsedTokens)
        {
            Value = value;
            UnparsedTokens = unparsedTokens;
        }

        public T Value { get; private set; }
        public Lexer UnparsedTokens { get; private set; }
        public bool Success { get { return true; } }
        public ErrorMessage ErrorMessage { get { return null; } }
        public Reply<U> ParseRest<U>(Func<T, Parser<U>> constructNextParser)
        {
            return constructNextParser(Value)(UnparsedTokens);
        }
    }
}