using System.Linq;
using Should;
using Xunit;

namespace Rook.Compiling
{
    public class InterpreterResultTests
    {
        [Fact]
        public void ShouldDescribeSuccessfulInterpretation()
        {
            object value = 123;
            var result = new InterpreterResult(value);

            result.Value.ShouldEqual(value);
            result.Errors.Count().ShouldEqual(0);
        }

        [Fact]
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