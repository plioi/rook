namespace Rook.Compiling.Syntax
{
    public interface Expression : TypedSyntaxTree
    {
        TypeChecked<Expression> WithTypes(Environment environment);
    }
}