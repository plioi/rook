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

        public TypeChecked<Expression> WithTypes(Environment environment)
        {
            TypeChecked<Expression> typeCheckedCondition = Condition.WithTypes(environment);
            TypeChecked<Expression> typeCheckedWhenTrue = BodyWhenTrue.WithTypes(environment);
            TypeChecked<Expression> typeCheckedWhenFalse = BodyWhenFalse.WithTypes(environment);

            var errors = new[] {typeCheckedCondition, typeCheckedWhenTrue, typeCheckedWhenFalse}.Errors();
            if (errors.Any())
                return TypeChecked<Expression>.Failure(errors);

            Expression typedCondition = typeCheckedCondition.Syntax;
            Expression typedWhenTrue = typeCheckedWhenTrue.Syntax;
            Expression typedWhenFalse = typeCheckedWhenFalse.Syntax;

            DataType typeOfCondition = typedCondition.Type;
            DataType typeWhenTrue = typedWhenTrue.Type;
            DataType typeWhenFalse = typedWhenFalse.Type;

            //TODO: Instead of using Position for the errors, attach Condition.Position or BodyWhenFalse.Position depending on which unification(s) failed.
            var normalizer = environment.TypeNormalizer;
            var unifyErrorsA = normalizer.Unify(NamedType.Boolean, typeOfCondition);
            var unifyErrorsB = normalizer.Unify(typeWhenTrue, typeWhenFalse);
            if (unifyErrorsA.Any() || unifyErrorsB.Any())
                return TypeChecked<Expression>.Failure(Position, unifyErrorsA.Concat(unifyErrorsB));

            return TypeChecked<Expression>.Success(new If(Position, typedCondition, typedWhenTrue, typedWhenFalse, typeWhenTrue));
        }

        public TResult Visit<TResult>(Visitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}