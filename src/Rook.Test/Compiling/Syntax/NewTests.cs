using Parsley;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class NewTests : ExpressionTests
    {
        [Fact]
        public void ConstructorCalls()
        {
            FailsToParse("new").AtEndOfInput().WithMessage("(1, 4): identifier expected");
            FailsToParse("new Foo").AtEndOfInput().WithMessage("(1, 8): ( expected");
            FailsToParse("new Foo(").AtEndOfInput().WithMessage("(1, 9): ) expected");
            Parses("new Foo()").IntoTree("(new Foo())");
        }
    }
}