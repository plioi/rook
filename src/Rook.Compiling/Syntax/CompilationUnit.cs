using System.Collections.Generic;
using Parsley;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class CompilationUnit : SyntaxTree
    {
        public Position Position { get; private set; }
        public Vector<Class> Classes { get; private set; }
        public Vector<Function> Functions { get; private set; }

        public CompilationUnit(Position position, IEnumerable<Class> classes, IEnumerable<Function> functions)
        {
            Position = position;
            Classes = classes.ToVector();
            Functions = functions.ToVector();
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public TypeChecked<CompilationUnit> WithTypes(TypeChecker visitor)
        {
            return visitor.TypeCheck(this);
        }
    }
}