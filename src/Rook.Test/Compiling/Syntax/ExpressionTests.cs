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

        protected DataType Type(string source, Scope scope, TypeUnifier unifier)
        {
            return TypeChecking(source, scope, unifier).Syntax.Type;
        }

        protected TypeChecked<Expression> TypeChecking(string source, params TypeMapping[] symbols)
        {
            var unifier = new TypeUnifier();
            return TypeChecking(source, Scope(unifier, symbols), unifier);
        }

        protected TypeChecked<Expression> TypeChecking(string source, Scope scope, TypeUnifier unifier)
        {
            var typeChecker = new TypeChecker();
            var expression = Parse(source);
            return typeChecker.TypeCheck(expression, scope, unifier);
        }

        protected T WithTypes<T>(T syntaxTree, params TypeMapping[] symbols) where T : Expression
        {
            var typeChecker = new TypeChecker();
            var unifier = new TypeUnifier();
            var typeCheckedExpression = typeChecker.TypeCheck(syntaxTree, Scope(unifier, symbols), unifier);
            return (T)typeCheckedExpression.Syntax;
        }
    }
}