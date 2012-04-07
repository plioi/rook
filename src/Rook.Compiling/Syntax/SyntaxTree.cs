using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public interface SyntaxTree
    {
        Position Position { get; }
        TResult Visit<TResult>(Visitor<TResult> visitor);
    }

    public interface TypedSyntaxTree : SyntaxTree
    {
        DataType Type { get; }
    }
}