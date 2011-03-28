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

            Assert.AreEqual(value, result.Value);
            Assert.AreEqual(0, result.Errors.Count());
        }

        [Test]
        public void ShouldDescribeFailedInterpretation()
        {
            CompilerError errorA = new CompilerError(1, 10, "Error A");
            CompilerError errorB = new CompilerError(2, 20, "Error B");
            var result = new InterpreterResult(errorA, errorB);

            Assert.IsNull(result.Value);
            Assert.AreEqual(new[] {errorA, errorB}, result.Errors.ToArray());
        }
    }
}