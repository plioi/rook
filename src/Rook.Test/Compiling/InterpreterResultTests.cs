using Parsley;
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
            result.Errors.ShouldBeEmpty();
            result.Language.ShouldEqual(Language.Rook);
        }

        [Fact]
        public void ShouldDescribeFailedInterpretation()
        {
            var errorA = new CompilerError(new Position(1, 10), "Error A");
            var errorB = new CompilerError(new Position(2, 20), "Error B");
            var result = new InterpreterResult(Language.CSharp, errorA, errorB);

            result.Value.ShouldBeNull();
            result.Errors.ShouldList(errorA, errorB);
            result.Language.ShouldEqual(Language.CSharp);
        }
    }
}