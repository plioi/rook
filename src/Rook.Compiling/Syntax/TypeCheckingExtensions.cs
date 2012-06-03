using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public static class TypeCheckingExtensions
    {
        public static Vector<T> ToVector<T>(this IEnumerable<T> items)
        {
            return new ArrayVector<T>(items.ToArray());
        }

        public static Vector<CompilerError> Errors<T>(this Vector<TypeChecked<T>> typeCheckedItems) where T : SyntaxTree
        {
            return typeCheckedItems.SelectMany(typeCheckedItem => typeCheckedItem.Errors).ToVector();
        }

        public static Vector<Class> Classes(this Vector<TypeChecked<Class>> typeCheckedClasses)
        {
            return typeCheckedClasses.Select(x => x.Syntax).ToVector();
        }

        public static Vector<Function> Functions(this Vector<TypeChecked<Function>> typeCheckedFunctions)
        {
            return typeCheckedFunctions.Select(x => x.Syntax).ToVector();
        }

        public static Vector<Expression> Expressions(this Vector<TypeChecked<Expression>> typeCheckedExpressions)
        {
            return typeCheckedExpressions.Select(x => x.Syntax).ToVector();
        }

        public static Vector<TypeChecked<Expression>> WithTypes(this Vector<Expression> expressions, Scope scope)
        {
            return expressions.Select(x => x.WithTypes(scope)).ToVector();
        }

        public static Vector<TypeChecked<Function>> WithTypes(this Vector<Function> functions, Scope scope)
        {
            return functions.Select(x => x.WithTypes(scope)).ToVector();
        } 

        public static Vector<TypeChecked<Class>> WithTypes(this Vector<Class> classes, Scope scope)
        {
            return classes.Select(x => x.WithTypes(scope)).ToVector();
        }

        public static Vector<CompilerError> Unify(this TypeNormalizer normalizer, DataType type, TypedSyntaxTree typedSyntaxTree)
        {
            return normalizer.Unify(type, typedSyntaxTree.Type)
                .Select(error => new CompilerError(typedSyntaxTree.Position, error)).ToVector();
        }
    }
}