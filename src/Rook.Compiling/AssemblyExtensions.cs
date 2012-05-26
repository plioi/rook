using System.Reflection;

namespace Rook.Compiling
{
    public static class AssemblyExtensions
    {
        public static object Execute(this Assembly assembly)
        {
            return assembly.GetType(ReservedName.__program__).GetMethod("Main").Invoke(null, null);
        }
    }
}
