using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public static class TypeCheckingExtensions
    {
        public static CompilerError[] Errors<T>(this IEnumerable<TypeChecked<T>> typeCheckedItems) where T : SyntaxTree
        {
            return typeCheckedItems.SelectMany(typeCheckedItem => typeCheckedItem.Errors).ToArray();
        }

        public static Class[] Classes(this IEnumerable<TypeChecked<Class>> typeCheckedClasses)
        {
            return typeCheckedClasses.Select(x => x.Syntax).ToArray();
        }

        public static Function[] Functions(this IEnumerable<TypeChecked<Function>> typeCheckedFunctions)
        {
            return typeCheckedFunctions.Select(x => x.Syntax).ToArray();
        }

        public static Expression[] Expressions(this IEnumerable<TypeChecked<Expression>> typeCheckedExpressions)
        {
            return typeCheckedExpressions.Select(x => x.Syntax).ToArray();
        }

        public static TypeChecked<Expression>[] WithTypes(this IEnumerable<Expression> expressions, Environment environment)
        {
            return expressions.Select(x => x.WithTypes(environment)).ToArray();
        }

        public static TypeChecked<Function>[] WithTypes(this IEnumerable<Function> functions, Environment environment)
        {
            return functions.Select(x => x.WithTypes(environment)).ToArray();
        } 

        public static TypeChecked<Class>[] WithTypes(this IEnumerable<Class> classes, Environment environment)
        {
            return classes.Select(x => x.WithTypes(environment)).ToArray();
        }

        public static DataType[] Types(this IEnumerable<Expression> expressions)
        {
            return expressions.Select(x => x.Type).ToArray();
        }
    }
}