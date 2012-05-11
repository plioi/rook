using System.Collections.Generic;
using System.Linq;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public class Program : SyntaxTree
    {
        public Position Position { get; private set; }
        public IEnumerable<Class> Classes { get; private set; }
        public IEnumerable<Function> Functions { get; private set; }

        public Program(Position position, IEnumerable<Class> classes, IEnumerable<Function> functions)
        {
            Position = position;
            Classes = classes;
            Functions = functions;
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public TypeChecked<Program> WithTypes()
        {
            var environment = new Environment();

            foreach (var @class in Classes)
                if (!environment.TryIncludeUniqueBinding(@class))
                    return TypeChecked<Program>.DuplicateIdentifierError(@class);

            foreach (var function in Functions)
                if (!environment.TryIncludeUniqueBinding(function))
                    return TypeChecked<Program>.DuplicateIdentifierError(function);

            var typeCheckedClasses = Classes.WithTypes(environment);

            //TODO: After nonempty classes are supported, uncomment the following Concat call.
            //      Empty classes could not possibly have errors of their own, so there is no
            //      way to write a unit test exposing the need for this Concat yet.

            var typeCheckedFunctions = Functions.WithTypes(environment);

            var errors = typeCheckedFunctions.Errors()/*.Concat(typeCheckedClasses.Errors())*/;
            if (errors.Any())
                return TypeChecked<Program>.Failure(errors);

            return TypeChecked<Program>.Success(new Program(Position, typeCheckedClasses.Classes(), typeCheckedFunctions.Functions()));
        }
    }
}