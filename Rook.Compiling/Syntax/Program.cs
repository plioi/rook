using System.Collections.Generic;
using System.Linq;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public sealed class Program : SyntaxTree
    {
        public Position Position { get; private set; }
        public IEnumerable<Function> Functions { get; private set; }

        public Program(Position position, IEnumerable<Function> functions)
        {
            Position = position;
            Functions = functions;
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        public TypeChecked<Program> WithTypes()
        {
            var environment = new Environment();

            foreach (var function in Functions)
                if (!environment.TryIncludeUniqueBinding(function))
                    return TypeChecked<Program>.DuplicateIdentifierError(function);

            IEnumerable<TypeChecked<Function>> typeCheckedFunctions = Functions.WithTypes(environment);

            var errors = typeCheckedFunctions.Errors();
            if (errors.Any())
                return TypeChecked<Program>.Failure(errors);

            return TypeChecked<Program>.Success(new Program(Position, typeCheckedFunctions.Functions()));
        }
    }
}