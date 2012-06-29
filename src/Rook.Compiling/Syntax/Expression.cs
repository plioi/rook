using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public interface Expression : TypedSyntaxTree
    {
        TypeChecked<Expression> WithTypes(TypeChecker visitor, Scope scope, TypeUnifier unifier);
    }
}