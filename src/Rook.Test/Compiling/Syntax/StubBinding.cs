using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class StubBinding : Binding
    {
        public StubBinding(string identifier, DataType type)
        {
            Identifier = identifier;
            Type = type;
        }

        public string Identifier { get; private set; }
        public DataType Type { get; private set; }
    }
}