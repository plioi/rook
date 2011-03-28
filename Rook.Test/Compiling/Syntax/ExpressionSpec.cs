using NUnit.Framework;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public abstract class ExpressionSpec : SyntaxTreeSpec<Expression>
    {
        protected override Parser<Expression> ParserUnderTest { get { return Grammar.Expression; } }
        
        protected void AssertType(DataType expectedType, string source, params TypeMapping[] symbols)
        {
            Assert.AreEqual(expectedType, TypeCheck(source, symbols).Syntax.Type);
        }

        protected void AssertType(DataType expectedType, string source, Environment environment)
        {
            Assert.AreEqual(expectedType, TypeCheck(source, environment).Syntax.Type);
        }

        protected void AssertTypeCheckError(int line, int column, string expectedMessage, string source, params TypeMapping[] symbols)
        {
            AssertTypeCheckError(TypeCheck(source, symbols), line, column, expectedMessage);
        }

        private TypeChecked<Expression> TypeCheck(string source, TypeMapping[] symbols)
        {
            return TypeCheck(source, Environment(symbols));
        }

        private TypeChecked<Expression> TypeCheck(string source, Environment environment)
        {
            return Parse(source).WithTypes(environment);
        }
    }
}