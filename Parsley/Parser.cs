namespace Parsley
{
    public delegate Parsed<T> Parser<out T>(Lexer tokens);
}