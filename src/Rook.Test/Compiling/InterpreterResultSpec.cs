using System.Linq;
using NUnit.Framework;

namespace Rook.Compiling
{
    [TestFixture]
    public class InterpreterResultSpec
    {
        [Test]
        public void ShouldDescribeSuccessfulInterpretation()
        {
            object value = 123;
            var result = new InterpreterResult(value);

            result.Value.ShouldEqual(value);
            result.Errors.Count().ShouldEqual(0);
        }

        [Test]
        public void ShouldDescribeFailedInterpretation()
        {
            var errorA = new CompilerError(1, 10, "Error A");
            var errorB = new CompilerError(2, 20, "Error B");
            var result = new InterpreterResult(errorA, errorB);

            result.Value.ShouldBeNull();
            result.Errors.ShouldList(errorA, errorB);
        }
    }
}