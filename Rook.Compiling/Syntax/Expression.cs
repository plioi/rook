using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public interface Expression : SyntaxTree
    {
        DataType Type { get; }
        TypeChecked<Expression> WithTypes(Environment environment);
    }
}