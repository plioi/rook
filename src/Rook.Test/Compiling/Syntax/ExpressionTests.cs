using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;
using Should;

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
            var expression = Parse(source);
            var typeCheckedExpression = typeChecker.TypeCheck(expression, Scope(typeChecker, symbols));

            typeCheckedExpression.ShouldNotBeNull();
            typeChecker.HasErrors.ShouldBeFalse();

            return typeCheckedExpression.Type;
        }

        protected DataType Type(string source, Scope scope, TypeChecker typeChecker)
        {
            var expression = Parse(source);
            return typeChecker.TypeCheck(expression, scope).Type;
        }

        protected Vector<CompilerError> ShouldFailTypeChecking(string source, params TypeMapping[] symbols)
        {
            return ShouldFailTypeChecking(source, new TypeChecker(), symbols);
        }

        protected Vector<CompilerError> ShouldFailTypeChecking(string source, TypeChecker typeChecker, params TypeMapping[] symbols)
        {
            var expression = Parse(source);
            var typeCheckedExpression = typeChecker.TypeCheck(expression, Scope(typeChecker, symbols));

            typeCheckedExpression.ShouldBeNull();
            typeChecker.HasErrors.ShouldBeTrue();

            return typeChecker.Errors;
        }

        protected T WithTypes<T>(T syntaxTree, params TypeMapping[] symbols) where T : Expression
        {
            return WithTypes(syntaxTree, new TypeChecker(), symbols);
        }

        protected T WithTypes<T>(T syntaxTree, TypeChecker typeChecker, params TypeMapping[] symbols) where T : Expression
        {
            var typeCheckedExpression = typeChecker.TypeCheck(syntaxTree, Scope(typeChecker, symbols));
            typeChecker.HasErrors.ShouldBeFalse();
            return (T)typeCheckedExpression;
        }
    }
}