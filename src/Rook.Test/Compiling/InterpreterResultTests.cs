using Parsley;
using Should;

namespace Rook.Compiling
{
    [Facts]
    public class InterpreterResultTests
    {
        public void ShouldDescribeSuccessfulInterpretation()
        {
            object value = 123;
            var result = new InterpreterResult(value);

            result.Value.ShouldEqual(value);
            result.Errors.ShouldBeEmpty();
            result.Language.ShouldEqual(Language.Rook);
        }

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