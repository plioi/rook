using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rook.Compiling.Types
{
    public static class TypeExtensions
    {
        public static string QualifiedName(this Type type)
        {
            return type.Namespace + "." + type.Name.Replace("`" + type.GetGenericArguments().Length, "");
        }
    }
}
