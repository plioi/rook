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
            return Parse(source).WithTypes(scope, unifier);
        }

        protected T WithTypes<T>(T syntaxTree, params TypeMapping[] symbols) where T : Expression
        {
            var unifier = new TypeUnifier();
            return (T)syntaxTree.WithTypes(Scope(unifier, symbols), unifier).Syntax;
        }
    }
}