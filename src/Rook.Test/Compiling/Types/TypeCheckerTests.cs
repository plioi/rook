using Rook.Compiling.Syntax;
using Should;
using Xunit;

namespace Rook.Compiling.Types
{
    public class TypeCheckerTests
    {
        private readonly TypeChecker typeChecker;
        private readonly TypeVariable x;
        private readonly TypeVariable y;
        private readonly TypeVariable z;

        public TypeCheckerTests()
        {
            typeChecker = new TypeChecker();
            x = typeChecker.CreateTypeVariable();
            y = typeChecker.CreateTypeVariable();
            z = typeChecker.CreateTypeVariable();
        }

        [Fact]
        public void ProvidesStreamOfUniqueTypeVariables()
        {
            x.ShouldEqual(new TypeVariable(0));
            y.ShouldEqual(new TypeVariable(1));
            z.ShouldEqual(new TypeVariable(2));
            typeChecker.CreateTypeVariable().ShouldEqual(new TypeVariable(3));
            typeChecker.CreateTypeVariable().ShouldEqual(new TypeVariable(4));
            typeChecker.CreateTypeVariable().ShouldEqual(new TypeVariable(5));
        }
    }
}