using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class TypeVariableSource
    {
        private int next;

        public TypeVariableSource()
        {
            next = 0;
        }

        public TypeVariable CreateGenericTypeVariable()
        {
            return new TypeVariable(next++, true);
        }

        public TypeVariable CreateNonGenericTypeVariable()
        {
            return new TypeVariable(next++, false);
        }
    }
}