using Parsley;

namespace Rook.Compiling.Syntax
{
    public interface SyntaxTree
    {
        Position Position { get; }
        TResult Visit<TResult>(Visitor<TResult> visitor);
    }
}