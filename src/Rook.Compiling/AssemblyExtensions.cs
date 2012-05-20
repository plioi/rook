using System.Reflection;

namespace Rook.Compiling
{
    public static class AssemblyExtensions
    {
        public static object Execute(this Assembly assembly)
        {
            return assembly.GetType("Program").GetMethod("Main").Invoke(null, null);
        }
    }
}
