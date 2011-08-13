using System;
using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class RookGrammar : Grammar
    {
        public static readonly GrammarRule<Program> Program = new GrammarRule<Program>();
        public static readonly GrammarRule<Function> Function = new GrammarRule<Function>();
        public static readonly GrammarRule<NamedType> TypeName = new GrammarRule<NamedType>();
        public static readonly GrammarRule<Expression> Expression = new GrammarRule<Expression>();
        public static readonly GrammarRule<Token> EndOfLine = new GrammarRule<Token>();
        public static readonly GrammarRule<Token> Identifier = new GrammarRule<Token>();
        public static readonly GrammarRule<Name> Name = new GrammarRule<Name>();

        #region Private Rules
        private static readonly GrammarRule<NamedType> NameType = new GrammarRule<NamedType>();
        private static readonly GrammarRule<Token> TypeModifier = new GrammarRule<Token>();
        private static readonly GrammarRule<NamedType> KeywordType = new GrammarRule<NamedType>();
        private static readonly GrammarRule<Parameter> ExplicitlyTypedParameter = new GrammarRule<Parameter>();
        private static readonly GrammarRule<Parameter> ImplicitlyTypedParameter = new GrammarRule<Parameter>();
        private static readonly GrammarRule<Parameter> Parameter = new GrammarRule<Parameter>();

        private static readonly GrammarRule<Expression> NullCoalescing = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Or = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> And = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Equality = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Relational = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Additive = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Multiplicative = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Unary = new GrammarRule<Expression>();
        
        private static readonly GrammarRule<Expression> Primary = new GrammarRule<Expression>();
        private static readonly GrammarRule<Func<Expression, Expression>> Call = new GrammarRule<Func<Expression, Expression>>();
        private static readonly GrammarRule<Func<Expression, Expression>> Subscript = new GrammarRule<Func<Expression, Expression>>();

        private static readonly GrammarRule<Expression> Literal = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> If = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Block = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Lambda = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> VectorLiteral = new GrammarRule<Expression>();
        private static readonly GrammarRule<VariableDeclaration> ExplicitlyTypedVariableDeclaration = new GrammarRule<VariableDeclaration>();
        private static readonly GrammarRule<VariableDeclaration> ImplicitlyTypedVariableDeclaration = new GrammarRule<VariableDeclaration>();
        private static readonly GrammarRule<VariableDeclaration> VariableDeclaration = new GrammarRule<VariableDeclaration>();
        private static readonly GrammarRule<Expression> BooleanLiteral = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> IntegerLiteral = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> NullLiteral = new GrammarRule<Expression>();
        #endregion

        static RookGrammar()
        {
            TopLevelConstructs();
            TypeNames();
            Parameters();
            Expressions();

            EndOfLine.Rule =
                Label(Choice(Token(RookLexer.EndOfLine), Token(Lexer.EndOfInput)), "end of line");

            Identifier.Rule =
                Token(RookLexer.Identifier);
        }

        private static void TopLevelConstructs()
        {
            Program.Rule =
                from leadingEndOfLine in Optional(Token(RookLexer.EndOfLine))
                from functions in ZeroOrMore(Function.TerminatedBy(EndOfLine)).TerminatedBy(EndOfInput)
                select new Program(new Position(1, 1), functions);

            Function.Rule =
                from returnType in TypeName
                from name in Name
                from parameters in Tuple(Parameter).TerminatedBy(Optional(EndOfLine))
                from body in Expression
                select new Function(name.Position, returnType, name, parameters, body);
        }

        private static void TypeNames()
        {
            NameType.Rule =
                from name in Name
                select new NamedType(name.Identifier);

            TypeModifier.Rule =
                Choice(Token("*"), Token("?"), Token("[]"));

            KeywordType.Rule =
                Choice(from _ in Token(RookLexer.@int) select NamedType.Integer,
                       from _ in Token(RookLexer.@bool) select NamedType.Boolean,
                       from _ in Token(RookLexer.@void) select NamedType.Void);

            TypeName.Rule =
                Label(from rootType in Choice(NameType, KeywordType)
                      from modifiers in ZeroOrMore(TypeModifier)
                      select modifiers.Aggregate(rootType, ApplyTypeModifier),
                      "type name");
        }

        private static void Parameters()
        {
            ExplicitlyTypedParameter.Rule =
                from type in TypeName
                from identifier in Identifier
                select new Parameter(identifier.Position, type, identifier.Literal);

            ImplicitlyTypedParameter.Rule =
                from identifier in Identifier
                select new Parameter(identifier.Position, identifier.Literal);

            Parameter.Rule =
                Choice(Attempt(ExplicitlyTypedParameter), ImplicitlyTypedParameter);
        }

        private static void Expressions()
        {
            Expression.Rule = NullCoalescing;
            NullCoalescing.Rule = Binary(Or, "??");
            Or.Rule = Binary(And, "||");
            And.Rule = Binary(Equality, "&&");
            Equality.Rule = Binary(Relational, "==", "!=");
            Relational.Rule = Binary(Additive, "<=", "<", ">=", ">");
            Additive.Rule = Binary(Multiplicative, "+", "-");
            Multiplicative.Rule = Binary(Unary, "*", "/");

            Unary.Rule =
                from symbol in Optional(Choice(Token("-"), Token("!")))
                from primary in Primary
                select symbol == null ? primary : new Call(symbol.Position, symbol.Literal, primary);

            var nothing = from zeroArguments in Zero<Expression>()
                          select new Func<Expression, Expression>(callable => callable);

            Primary.Rule =
                from callable in Choice(Literal, VectorLiteral, If, Block, Lambda, Name, Parenthesized(Expression))
                from callArgumentAppender in Choice(Attempt(Call), Attempt(Subscript), nothing)
                select callArgumentAppender(callable);

            Literal.Rule =
                Choice(BooleanLiteral, NullLiteral, IntegerLiteral);

            BooleanLiteral.Rule =
                from token in Choice(Token(RookLexer.@true), Token(RookLexer.@false))
                select new BooleanLiteral(token.Position, bool.Parse(token.Literal));

            IntegerLiteral.Rule =
                from token in Token(RookLexer.Integer)
                select new IntegerLiteral(token.Position, token.Literal);

            NullLiteral.Rule =
                from literal in Token(RookLexer.@null)
                select new Null(literal.Position);

            VectorLiteral.Rule =
                from items in Between("[", OneOrMore(Expression, Token(",")), "]")
                select new VectorLiteral(items.First().Position, items);

            If.Rule =
                from _if_ in Token(RookLexer.@if)
                from condition in Parenthesized(Expression).TerminatedBy(Optional(EndOfLine))
                from bodyWhenTrue in Expression.TerminatedBy(Optional(EndOfLine))
                from _else_ in Token(RookLexer.@else).TerminatedBy(Optional(EndOfLine))
                from bodyWhenFalse in Expression
                select new If(_if_.Position, condition, bodyWhenTrue, bodyWhenFalse);

            Block.Rule =
                from open in Token("{").TerminatedBy(Optional(EndOfLine))
                from variableDeclarations in ZeroOrMore(Attempt(VariableDeclaration))
                from innerExpressions in OneOrMore(Expression.TerminatedBy(EndOfLine))
                from close in Token("}")
                select new Block(open.Position, variableDeclarations, innerExpressions);

            Lambda.Rule =
                from _fn_ in Token(RookLexer.@fn)
                from parameters in Tuple(Parameter)
                from body in Expression
                select new Lambda(_fn_.Position, parameters, body);

            Name.Rule =
                from identifier in Identifier
                select new Name(identifier.Position, identifier.Literal);

            Call.Rule =
                from arguments in Tuple(Expression)
                select new Func<Expression, Expression>(callable => new Call(callable.Position, callable, arguments));

            Subscript.Rule =
                from arguments in Between("[", OneOrMore(Expression, Token(":")), "]")
                select new Func<Expression, Expression>(
                    callable => new Call(callable.Position,
                                         new Name(callable.Position, arguments.Count() == 1 ? "Index" : "Slice"),
                                         new[] {callable}.Concat(arguments)));

            VariableDeclaration.Rule =
                Choice(Attempt(ExplicitlyTypedVariableDeclaration), ImplicitlyTypedVariableDeclaration);

            ExplicitlyTypedVariableDeclaration.Rule =
                from type in TypeName
                from identifier in Identifier
                from assignment in Token("=")
                from value in Expression
                from end in EndOfLine
                select new VariableDeclaration(identifier.Position, type, identifier.Literal, value);

            ImplicitlyTypedVariableDeclaration.Rule =
                from identifier in Identifier
                from assignment in Token("=")
                from value in Expression
                from end in EndOfLine
                select new VariableDeclaration(identifier.Position, identifier.Literal, value);
        }

        private static Parser<T> Between<T>(string openOperator, Parser<T> parser, string closeOperator)
        {
            return Between(Token(openOperator), parser, Token(closeOperator));
        }

        private static Parser<T> Parenthesized<T>(Parser<T> parser)
        {
            return Between("(", parser, ")");
        }

        private static Parser<IEnumerable<T>> Tuple<T>(Parser<T> item)
        {
            return Parenthesized(ZeroOrMore(item, Token(",")));
        }

        private static NamedType ApplyTypeModifier(NamedType targetType, Token modifier)
        {
            if (modifier.Literal == "*")
                return NamedType.Enumerable(targetType);

            if (modifier.Literal == "[]")
                return NamedType.Vector(targetType);

            return NamedType.Nullable(targetType);
        }

        private static Parser<Expression> Binary(Parser<Expression> operand, params string[] symbols)
        {
            Parser<Token>[] symbolParsers = symbols.Select(Token).ToArray();

            return LeftAssociative(operand, Choice(symbolParsers),
                                   (left, symbolAndRight) =>
                                   new Call(symbolAndRight.Item1.Position, symbolAndRight.Item1.Literal, left,
                                            symbolAndRight.Item2));
        }
    }
}