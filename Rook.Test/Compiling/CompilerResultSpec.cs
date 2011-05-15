using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Rook.Compiling
{
    [TestFixture]
    public class CompilerResultSpec
    {
        [Test]
        public void ShouldDescribeSuccessfulCompilation()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var result = new CompilerResult(assembly);

            result.CompiledAssembly.ShouldEqual(assembly);
            result.Errors.Count().ShouldEqual(0);
        }

        [Test]
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
