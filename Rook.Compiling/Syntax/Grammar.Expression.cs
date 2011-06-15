﻿using System;
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
                return from symbol in Optional(Operator("-", "!"))
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

                var subscript = from arguments in Between("[", OneOrMore(Expression, Operator(":")), "]")
                                select new Func<Expression, Expression>(
                                    callable =>
                                        new Call(callable.Position,
                                             new Name(callable.Position, arguments.Count() == 1 ? "Index" : "Slice"),
                                             new[] {callable}.Concat(arguments)));

                var nothing = from zeroArguments in Zero<Expression>()
                              select new Func<Expression, Expression>(callable => callable);

                return from callable in GreedyChoice(Literal, VectorLiteral, If, Block, Lambda, Name, Parenthesized(Expression))
                       from callArgumentAppender in GreedyChoice(call, subscript, nothing)
                       select callArgumentAppender(callable);
            }
        }

        private static Parser<Expression> Literal
        {
            get { return GreedyChoice(BooleanLiteral, NullLiteral, IntegerLiteral); }
        }

        private static Parser<Expression> If
        {
            get
            {
                return from @if in Keyword("if")
                       from condition in Parenthesized(Expression).TerminatedBy(Optional(EndOfLine))
                       from bodyWhenTrue in Expression.TerminatedBy(Optional(EndOfLine))
                       from @else in Keyword("else").TerminatedBy(Optional(EndOfLine))
                       from bodyWhenFalse in Expression
                       select new If(@if.Position, condition, bodyWhenTrue, bodyWhenFalse);
            }
        }

        private static Parser<Expression> Block
        {
            get
            {
                return from open in Operator("{").TerminatedBy(Optional(EndOfLine))
                       from variableDeclarations in ZeroOrMore(VariableDeclaration)
                       from innerExpressions in OneOrMore(Expression.TerminatedBy(EndOfLine))
                       from close in Operator("}")
                       select new Block(open.Position, variableDeclarations, innerExpressions);
            }
        }

        private static Parser<Expression> Lambda
        {
            get
            {
                return from fn in Keyword("fn")
                       from parameters in Tuple(Parameter)
                       from body in Expression
                       select new Lambda(fn.Position, parameters, body);
            }
        }

        private static Parser<Expression> VectorLiteral
        {
            get
            {
                return from items in Between("[", OneOrMore(Expression, Operator(",")), "]")
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
                    from assignment in Operator("=")
                    from value in Expression
                    from end in EndOfLine
                    select new VariableDeclaration(identifier.Position, type, identifier.Literal, value);

                Parser<VariableDeclaration> implicitlyTyped =
                    from identifier in Identifier
                    from assignment in Operator("=")
                    from value in Expression
                    from end in EndOfLine
                    select new VariableDeclaration(identifier.Position, identifier.Literal, value);

                return GreedyChoice(explicitlyTyped, implicitlyTyped);
            }
        }

        private static Parser<Expression> BooleanLiteral
        {
            get
            {
                return from token in Boolean
                       select new BooleanLiteral(token.Position, bool.Parse(token.Literal));
            }
        }

        private static Parser<Expression> IntegerLiteral
        {
            get
            {
                return from token in Integer
                       select new IntegerLiteral(token.Position, token.Literal);
            }
        }

        private static Parser<Expression> NullLiteral
        {
            get
            {
                return from literal in Keyword("null")
                       select new Null(literal.Position);
            }
        }

        private static Parser<Expression> Binary(Parser<Expression> operand, params string[] symbols)
        {
            return from leftAssociative in
                       LeftAssociative(operand, Operator(symbols),
                                       (left, symbolAndRight) =>
                                       new Call(symbolAndRight.Item1.Position, symbolAndRight.Item1.Literal, left, symbolAndRight.Item2))
                   select leftAssociative;
        }
    }
}