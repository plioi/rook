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

        protected DataType Type(string source, Scope scope)
        {
            return TypeChecking(source, scope).Syntax.Type;
        }

        protected TypeChecked<Expression> TypeChecking(string source, params TypeMapping[] symbols)
        {
            return TypeChecking(source, Scope(symbols));
        }

        protected TypeChecked<Expression> TypeChecking(string source, Scope scope)
        {
            return Parse(source).WithTypes(scope);
        }
    }
}