namespace Rook.Compiling.Syntax
{
    public interface Expression : TypedSyntaxTree
    {
        Expression WithTypes(TypeChecker visitor, Scope scope);
    }
}