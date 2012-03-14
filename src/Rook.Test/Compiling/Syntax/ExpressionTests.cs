using Parsley;
using Rook.Compiling.Types;
using Should;

namespace Rook.Compiling.Syntax
{
    public abstract class ExpressionTests : SyntaxTreeTests<Expression>
    {
        protected override Parser<Expression> Parser { get { return RookGrammar.Expression; } }
        
        protected void AssertType(DataType expectedType, string source, params TypeMapping[] symbols)
        {
            TypeCheck(source, symbols).Syntax.Type.ShouldEqual(expectedType);
        }

        protected void AssertType(DataType expectedType, string source, Environment environment)
        {
            TypeCheck(source, environment).Syntax.Type.ShouldEqual(expectedType);
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