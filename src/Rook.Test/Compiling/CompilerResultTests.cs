using System.Reflection;
using Parsley;
using Should;

namespace Rook.Compiling
{
    [Facts]
    public class CompilerResultTests
    {
        public void ShouldDescribeSuccessfulCompilation()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var result = new CompilerResult(assembly);

            result.CompiledAssembly.ShouldEqual(assembly);
            result.Errors.ShouldBeEmpty();
            result.Language.ShouldEqual(Language.Rook);
        }

        public void ShouldDescribeFailedCompilation()
        {
            var errorA = new CompilerError(new Position(1, 10), "Error A");
            var errorB = new CompilerError(new Position(2, 20), "Error B");
            var result = new CompilerResult(Language.CSharp, errorA, errorB);

            result.CompiledAssembly.ShouldBeNull();
            result.Errors.ShouldList(errorA, errorB);
            result.Language.ShouldEqual(Language.CSharp);
        }
    }
}
