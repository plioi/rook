using System.Linq;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class If : Expression
    {
        public Position Position { get; private set; }
        public Expression Condition { get; private set; }
        public Expression BodyWhenTrue { get; private set; }
        public Expression BodyWhenFalse { get; private set; }
        public DataType Type { get; private set; }

        public If(Position position, Expression condition, Expression bodyWhenTrue, Expression bodyWhenFalse)
            : this(position, condition, bodyWhenTrue, bodyWhenFalse, null) { }

        private If(Position position, Expression condition, Expression bodyWhenTrue, Expression bodyWhenFalse, DataType type)
        {
            Position = position;
            Condition = condition;
            BodyWhenTrue = bodyWhenTrue;
            BodyWhenFalse = bodyWhenFalse;
            Type = type;
        }

        public TypeChecked<Expression> WithTypes(Scope scope)
        {
            TypeChecked<Expression> typeCheckedCondition = Condition.WithTypes(scope);
            TypeChecked<Expression> typeCheckedWhenTrue = BodyWhenTrue.WithTypes(scope);
            TypeChecked<Expression> typeCheckedWhenFalse = BodyWhenFalse.WithTypes(scope);

            if (typeCheckedCondition.HasErrors || typeCheckedWhenTrue.HasErrors || typeCheckedWhenFalse.HasErrors)
                return TypeChecked<Expression>.Failure(new[] {typeCheckedCondition, typeCheckedWhenTrue, typeCheckedWhenFalse}.ToVector().Errors());

            Expression typedCondition = typeCheckedCondition.Syntax;
            Expression typedWhenTrue = typeCheckedWhenTrue.Syntax;
            Expression typedWhenFalse = typeCheckedWhenFalse.Syntax;

            var normalizer = scope.TypeNormalizer;
            var unifyErrorsA = normalizer.Unify(NamedType.Boolean, typedCondition);
            var unifyErrorsB = normalizer.Unify(typedWhenTrue.Type, typedWhenFalse);

            if (unifyErrorsA.Any() || unifyErrorsB.Any())
                return TypeChecked<Expression>.Failure(unifyErrorsA.Concat(unifyErrorsB).ToVector());

            return TypeChecked<Expression>.Success(new If(Position, typedCondition, typedWhenTrue, typedWhenFalse, typedWhenTrue.Type));
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}