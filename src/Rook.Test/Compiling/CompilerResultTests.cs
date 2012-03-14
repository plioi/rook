using System.Linq;
using System.Reflection;
using Should;
using Xunit;

namespace Rook.Compiling
{
    public class CompilerResultTests
    {
        [Fact]
        public void ShouldDescribeSuccessfulCompilation()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var result = new CompilerResult(assembly);

            result.CompiledAssembly.ShouldEqual(assembly);
            result.Errors.Count().ShouldEqual(0);
        }

        [Fact]
        public void ShouldDescribeFailedCompilation()
        {
            var errorA = new CompilerError(1, 10, "Error A");
            var errorB = new CompilerError(2, 20, "Error B");
            var result = new CompilerResult(errorA, errorB);

            result.CompiledAssembly.ShouldBeNull();
            result.Errors.ShouldList(errorA, errorB);
        }
    }
}
