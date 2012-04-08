using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public abstract class ExpressionTests : SyntaxTreeTests<Expression>
    {
        protected override Parser<Expression> Parser { get { return RookGrammar.Expression; } }

        protected DataType Type(string source, params TypeMapping[] symbols)
        {
            return TypeChecking(source, symbols).Syntax.Type;
        }

        protected DataType Type(string source, Environment environment)
        {
            return TypeChecking(source, environment).Syntax.Type;
        }

        protected TypeChecked<Expression> TypeChecking(string source, params TypeMapping[] symbols)
        {
            return TypeChecking(source, Environment(symbols));
        }

        protected TypeChecked<Expression> TypeChecking(string source, Environment environment)
        {
            return Parse(source).WithTypes(environment);
        }
    }
}