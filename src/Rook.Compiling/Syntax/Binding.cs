using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public interface Binding
    {
        string Identifier { get; }
        DataType Type { get; }
    }
}