using Parsley;
using Xunit;

namespace Rook.Compiling.Syntax
{
    public class ClassTests : SyntaxTreeTests<Class>
    {
        protected override Parser<Class> Parser { get { return RookGrammar.Class; } }

        [Fact]
        public void ParsesEmptyClassDeclarations()
        {
            FailsToParse("").AtEndOfInput().WithMessage("(1, 1): class expected");
            FailsToParse("class").AtEndOfInput().WithMessage("(1, 6): identifier expected");
            Parses("class Foo").IntoTree("class Foo");
        }
    }
}