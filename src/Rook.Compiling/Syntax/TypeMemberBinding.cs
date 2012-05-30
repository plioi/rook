using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public interface TypeMemberBinding
    {
        DataType Type { get; }
        Vector<Binding> Members { get; }
    }
}