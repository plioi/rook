using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Rook.Core.Collections;

namespace Rook.Compiling.Syntax
{
    public class VectorLiteral : Expression
    {
        public Position Position { get; private set; }
        public Vector<Expression> Items { get; private set; }
        public DataType Type { get; private set; }

        public VectorLiteral(Position position, IEnumerable<Expression> items)
            : this(position, items.ToVector(), null) { }

        private VectorLiteral(Position position, Vector<Expression> items, DataType type)
        {
            Position = position;
            Items = items;
            Type = type;
        }

        public TypeChecked<Expression> WithTypes(Scope scope)
        {
            var typeCheckedItems = Items.WithTypes(scope);

            var errors = typeCheckedItems.Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            var typedItems = typeCheckedItems.Expressions();

            var firstItemType = typedItems.First().Type;

            var normalizer = scope.TypeNormalizer;
            var unifyErrors = new List<CompilerError>();
            foreach (var typedItem in typedItems)
                unifyErrors.AddRange(normalizer.Unify(firstItemType, typedItem));

            if (unifyErrors.Count > 0)
                return TypeChecked<Expression>.Failure(unifyErrors.ToVector());

            return TypeChecked<Expression>.Success(new VectorLiteral(Position, typedItems, NamedType.Vector(firstItemType)));
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}