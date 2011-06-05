using System;

namespace Parsley
{
    public interface Parsed<out T>
    {
        T Value { get; }
        Lexer UnparsedTokens { get; }
        bool IsError { get; }
        string Message { get; }
        Parsed<U> ParseRest<U>(Func<T, Parser<U>> constructNextParser);
    }
}