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

        public TypeChecked<Expression> WithTypes(Environment environment)
        {
            var typeCheckedItems = Items.WithTypes(environment);

            var errors = typeCheckedItems.Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            var typedItems = typeCheckedItems.Expressions();
            var types = typedItems.Types();

            var firstItemType = types.First();

            //TODO: Instead of using Position in the errors, use the itemType.Position of the unification(s) that failed.
            var normalizer = environment.TypeNormalizer;
            var unifyErrors = new List<string>();
            foreach (var itemType in types)
                unifyErrors.AddRange(normalizer.Unify(firstItemType, itemType));

            if (unifyErrors.Any())
                return TypeChecked<Expression>.Failure(Position, unifyErrors);

            return TypeChecked<Expression>.Success(new VectorLiteral(Position, typedItems, NamedType.Vector(firstItemType)));
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}