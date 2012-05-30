using System.Collections.Generic;
using System.Linq;
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

        public TypeChecked<CompilationUnit> WithTypes()
        {
            var rootEnvironment = new Environment();
            var environment = Environment.CreateEnvironmentWithBuiltins(rootEnvironment);

            foreach (var @class in Classes)
                if (!environment.TryIncludeUniqueBinding(@class))
                    return TypeChecked<CompilationUnit>.DuplicateIdentifierError(@class);

            foreach (var function in Functions)
                if (!environment.TryIncludeUniqueBinding(function))
                    return TypeChecked<CompilationUnit>.DuplicateIdentifierError(function);

            var typeCheckedClasses = Classes.WithTypes(environment);
            var typeCheckedFunctions = Functions.WithTypes(environment);

            var classErrors = typeCheckedClasses.Errors();
            var functionErrors = typeCheckedFunctions.Errors();

            if (classErrors.Any() || functionErrors.Any())
                return TypeChecked<CompilationUnit>.Failure(classErrors.Concat(functionErrors).ToVector());

            return TypeChecked<CompilationUnit>.Success(new CompilationUnit(Position, typeCheckedClasses.Classes(), typeCheckedFunctions.Functions()));
        }
    }
}