using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public interface Binding
    {
        string Identifier { get; }
        DataType Type { get; }
    }

    public static class BindingExtensions
    {
        public static bool IsImplicitlyTyped(this Binding binding)
        {
            return binding.Type == null;
        }
    }
}