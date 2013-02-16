using System;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling
{
    [Obsolete]
    public class MethodBinding : Binding
    {
        public MethodBinding(string identifier, DataType type)
        {
            Identifier = identifier;
            Type = type;
        }

        public string Identifier { get; private set; }
        public DataType Type { get; private set; }
    }
}