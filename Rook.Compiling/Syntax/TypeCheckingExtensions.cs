using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public static class TypeCheckingExtensions
    {
        public static IEnumerable<CompilerError> Errors<T>(this IEnumerable<TypeChecked<T>> typeCheckedItems) where T : SyntaxTree
        {
            return typeCheckedItems.SelectMany(typeCheckedItem => typeCheckedItem.Errors);
        }

        public static IEnumerable<Function> Functions(this IEnumerable<TypeChecked<Function>> typeCheckedFunctions)
        {
            return typeCheckedFunctions.Select(x => x.Syntax);
        }

        public static IEnumerable<Expression> Expressions(this IEnumerable<TypeChecked<Expression>> typeCheckedExpressions)
        {
            return typeCheckedExpressions.Select(x => x.Syntax);
        }

        public static IEnumerable<TypeChecked<Expression>> WithTypes(this IEnumerable<Expression> expressions, Environment environment)
        {
            return expressions.Select(x => x.WithTypes(environment)).ToArray();
        }

        public static IEnumerable<TypeChecked<Function>> WithTypes(this IEnumerable<Function> functions, Environment environment)
        {
            return functions.Select(x => x.WithTypes(environment)).ToArray();
        }

        public static IEnumerable<DataType> Types(this IEnumerable<Expression> expressions)
        {
            return expressions.Select(x => x.Type);
        }
    }
}