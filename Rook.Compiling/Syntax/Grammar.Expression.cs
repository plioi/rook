using System;
using System.Linq;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public partial class Grammar
    {
        public static Parser<Expression> Expression
        {
            get
            {
                Parser<Expression> Multiplicative = Binary(Unary, "*", "/");
                Parser<Expression> Additive = Binary(Multiplicative, "+", "-");
                Parser<Expression> Relational = Binary(Additive, "<=", "<", ">=", ">");
                Parser<Expression> Equality = Binary(Relational, "==", "!=");
                Parser<Expression> And = Binary(Equality, "&&");
                Parser<Expression> Or = Binary(And, "||");
                Parser<Expression> NullCoalescing = Binary(Or, "??");
                return NullCoalescing;
            }
        }

        private static Parser<Expression> Unary
        {
            get
            {
                return from symbol in Optional(Choice(Token("-"), Token("!")))
                       from primary in Primary
                       select symbol == null ? primary : new Call(symbol.Position, symbol.Literal, primary);
            }
        }

        private static Parser<Expression> Primary
        {
            get
            {
                Parser<Func<Expression, Expression>> call = from arguments in Tuple(Expression)
                           select new Func<Expression, Expression>(callable => new Call(callable.Position, callable, arguments));

                var subscript = from arguments in Between("[", OneOrMore(Expression, Token(":")), "]")
                                select new Func<Expression, Expression>(
                                    callable =>
                                        new Call(callable.Position,
                                             new Name(callable.Position, arguments.Count() == 1 ? "Index" : "Slice"),
                                             new[] {callable}.Concat(arguments)));

                var nothing = from zeroArguments in Zero<Expression>()
                              select new Func<Expression, Expression>(callable => callable);

                return from callable in Choice(Literal, VectorLiteral, If, Block, Lambda, Name, Parenthesized(Expression))
                       from callArgumentAppender in Choice(Attempt(call), Attempt(subscript), nothing)
                       select callArgumentAppender(callable);
            }
        }

        private static Parser<Expression> Literal
        {
            get { return Choice(BooleanLiteral, NullLiteral, IntegerLiteral); }
        }

        private static Parser<Expression> If
        {
            get
            {
                return from _if_ in Token(RookLexer.@if)
                       from condition in Parenthesized(Expression).TerminatedBy(Optional(EndOfLine))
                       from bodyWhenTrue in Expression.TerminatedBy(Optional(EndOfLine))
                       from _else_ in Token(RookLexer.@else).TerminatedBy(Optional(EndOfLine))
                       from bodyWhenFalse in Expression
                       select new If(_if_.Position, condition, bodyWhenTrue, bodyWhenFalse);
            }
        }

        private static Parser<Expression> Block
        {
            get
            {
                return from open in Token("{").TerminatedBy(Optional(EndOfLine))
                       from variableDeclarations in ZeroOrMore(Attempt(VariableDeclaration))
                       from innerExpressions in OneOrMore(Expression.TerminatedBy(EndOfLine))
                       from close in Token("}")
                       select new Block(open.Position, variableDeclarations, innerExpressions);
            }
        }

        private static Parser<Expression> Lambda
        {
            get
            {
                return from _fn_ in Token(RookLexer.@fn)
                       from parameters in Tuple(Parameter)
                       from body in Expression
                       select new Lambda(_fn_.Position, parameters, body);
            }
        }

        private static Parser<Expression> VectorLiteral
        {
            get
            {
                return from items in Between("[", OneOrMore(Expression, Token(",")), "]")
                       select new VectorLiteral(items.First().Position, items);
            }
        }

        private static Parser<VariableDeclaration> VariableDeclaration
        {
            get
            {
                Parser<VariableDeclaration> explicitlyTyped =
                    from type in TypeName
                    from identifier in Identifier
                    from assignment in Token("=")
                    from value in Expression
                    from end in EndOfLine
                    select new VariableDeclaration(identifier.Position, type, identifier.Literal, value);

                Parser<VariableDeclaration> implicitlyTyped =
                    from identifier in Identifier
                    from assignment in Token("=")
                    from value in Expression
                    from end in EndOfLine
                    select new VariableDeclaration(identifier.Position, identifier.Literal, value);

                return Choice(Attempt(explicitlyTyped), implicitlyTyped);
            }
        }

        private static Parser<Expression> BooleanLiteral
        {
            get
            {
                return from token in Choice(Token(RookLexer.@true), Token(RookLexer.@false))
                       select new BooleanLiteral(token.Position, bool.Parse(token.Literal));
            }
        }

        private static Parser<Expression> IntegerLiteral
        {
            get
            {
                return from token in Token(RookLexer.Integer)
                       select new IntegerLiteral(token.Position, token.Literal);
            }
        }

        private static Parser<Expression> NullLiteral
        {
            get
            {
                return from literal in Token(RookLexer.@null)
                       select new Null(literal.Position);
            }
        }

        private static Parser<Expression> Binary(Parser<Expression> operand, params string[] symbols)
        {
            Parser<Token>[] symbolParsers = symbols.Select(Token).ToArray();

            return from leftAssociative in
                       LeftAssociative(operand, Choice(symbolParsers),
                                       (left, symbolAndRight) =>
                                       new Call(symbolAndRight.Item1.Position, symbolAndRight.Item1.Literal, left, symbolAndRight.Item2))
                   select leftAssociative;
        }
    }
}