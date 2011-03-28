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

            Assert.AreEqual(assembly, result.CompiledAssembly);
            Assert.AreEqual(0, result.Errors.Count());
        }

        [Test]
        public void ShouldDescribeFailedCompilation()
        {
            CompilerError errorA = new CompilerError(1, 10, "Error A");
            CompilerError errorB = new CompilerError(2, 20, "Error B");
            var result = new CompilerResult(errorA, errorB);

            Assert.IsNull(result.CompiledAssembly);
            Assert.AreEqual(new[] { errorA, errorB }, result.Errors.ToArray());
        }
    }
}
