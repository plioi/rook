using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public abstract class ExpressionTests : SyntaxTreeTests<Expression>
    {
        protected override Parser<Expression> Parser { get { return RookGrammar.Expression; } }

        protected DataType Type(string source, params TypeMapping[] symbols)
        {
            return Type(source, new TypeChecker(), symbols);
        }

        protected DataType Type(string source, TypeChecker typeChecker, params TypeMapping[] symbols)
        {
            return TypeChecking(source, typeChecker, symbols).Syntax.Type;
        }

        protected DataType Type(string source, Scope scope, TypeChecker typeChecker)
        {
            var expression = Parse(source);
            return typeChecker.TypeCheck(expression, scope).Syntax.Type;
        }

        protected TypeChecked<Expression> TypeChecking(string source, params TypeMapping[] symbols)
        {
            return TypeChecking(source, new TypeChecker(), symbols);
        }

        protected TypeChecked<Expression> TypeChecking(string source, TypeChecker typeChecker, params TypeMapping[] symbols)
        {
            var expression = Parse(source);
            return typeChecker.TypeCheck(expression, Scope(typeChecker, symbols));
        }

        protected T WithTypes<T>(T syntaxTree, params TypeMapping[] symbols) where T : Expression
        {
            return WithTypes(syntaxTree, new TypeChecker(), symbols);
        }

        protected T WithTypes<T>(T syntaxTree, TypeChecker typeChecker, params TypeMapping[] symbols) where T : Expression
        {
            var typeCheckedExpression = typeChecker.TypeCheck(syntaxTree, Scope(typeChecker, symbols));
            return (T)typeCheckedExpression.Syntax;
        }
    }
}