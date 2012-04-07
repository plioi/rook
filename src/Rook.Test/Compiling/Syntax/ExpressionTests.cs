using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public abstract class ExpressionTests : SyntaxTreeTests<Expression>
    {
        protected override Parser<Expression> Parser { get { return RookGrammar.Expression; } }

        protected DataType Type(string source, params TypeMapping[] symbols)
        {
            return TypeCheck(source, symbols).Syntax.Type;
        }

        protected DataType Type(string source, Environment environment)
        {
            return TypeCheck(source, environment).Syntax.Type;
        }

        protected void AssertTypeCheckError(int line, int column, string expectedMessage, string source, params TypeMapping[] symbols)
        {
            var expectedPosition = new Position(line, column);
            AssertTypeCheckError(TypeCheck(source, symbols), expectedPosition, expectedMessage);
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