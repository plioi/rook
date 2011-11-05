using System.Collections.Generic;
using System.Linq;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public class RookGrammar : Grammar
    {
        public readonly GrammarRule<Program> Program = new GrammarRule<Program>();
        public readonly GrammarRule<Function> Function = new GrammarRule<Function>();
        public readonly GrammarRule<NamedType> TypeName = new GrammarRule<NamedType>();
        public readonly GrammarRule<Token> EndOfLine = new GrammarRule<Token>();
        public readonly GrammarRule<Token> Identifier = new GrammarRule<Token>();
        public readonly GrammarRule<Name> Name = new GrammarRule<Name>();

        private readonly GrammarRule<NamedType> NameType = new GrammarRule<NamedType>();
        private readonly GrammarRule<Token> TypeModifier = new GrammarRule<Token>();
        private readonly GrammarRule<NamedType> KeywordType = new GrammarRule<NamedType>();
        private readonly GrammarRule<Parameter> ExplicitlyTypedParameter = new GrammarRule<Parameter>();
        private readonly GrammarRule<Parameter> ImplicitlyTypedParameter = new GrammarRule<Parameter>();
        public readonly GrammarRule<Parameter> Parameter = new GrammarRule<Parameter>();

        public readonly GrammarRule<Expression> Parenthetical = new GrammarRule<Expression>();
        public readonly GrammarRule<Expression> If = new GrammarRule<Expression>();
        public readonly GrammarRule<Expression> Lambda = new GrammarRule<Expression>();
        public readonly GrammarRule<Expression> VectorLiteral = new GrammarRule<Expression>();
        public readonly GrammarRule<Expression> Block = new GrammarRule<Expression>();
        public readonly GrammarRule<VariableDeclaration> VariableDeclaration = new GrammarRule<VariableDeclaration>();
        public readonly GrammarRule<VariableDeclaration> ExplicitlyTypedVariableDeclaration = new GrammarRule<VariableDeclaration>();
        public readonly GrammarRule<VariableDeclaration> ImplicitlyTypedVariableDeclaration = new GrammarRule<VariableDeclaration>();
        public readonly OperatorPrecedenceParser<Expression> Expression = new OperatorPrecedenceParser<Expression>();

        public RookGrammar()
        {
            InferGrammarRuleNames();

            TopLevelConstructs();
            TypeNames();
            Parameters();
            Expressions();

            EndOfLine.Rule =
                Label(Choice(Token(RookLexer.EndOfLine), Token(Lexer.EndOfInput)), "end of line");

            Identifier.Rule =
                Token(RookLexer.Identifier);
        }

        private void TopLevelConstructs()
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

        private void TypeNames()
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

        private void Parameters()
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

        private void Expressions()
        {
            Parenthetical.Rule =
                Between("(", Expression, ")");

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

            VectorLiteral.Rule =
                from items in Between("[", OneOrMore(Expression, Token(",")), "]")
                select new VectorLiteral(items.First().Position, items);

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

            Atom(RookLexer.@null, token => new Null(token.Position));
            Atom(RookLexer.Identifier, token => new Name(token.Position, token.Literal));
            Atom(RookLexer.Integer, token => new IntegerLiteral(token.Position, token.Literal));
            Atom(RookLexer.@true, token => new BooleanLiteral(token.Position, true));
            Atom(RookLexer.@false, token => new BooleanLiteral(token.Position, false));

            Unit(RookLexer.LeftSquareBrace, VectorLiteral);
            Unit(RookLexer.@if, If);
            Unit(RookLexer.fn, Lambda);
            Unit(RookLexer.LeftBrace, Block);
            Unit(RookLexer.LeftParen, Parenthetical);

            Extend(RookLexer.LeftParen, 12, Call);
            Extend(RookLexer.LeftSquareBrace, 12, Subscript);

            Prefix(RookLexer.Subtract, 11);
            Prefix(RookLexer.Not, 11);

            Binary(RookLexer.Multiply, 10);
            Binary(RookLexer.Divide, 10);

            Binary(RookLexer.Add, 9);
            Binary(RookLexer.Subtract, 9);

            Binary(RookLexer.LessThan, 8);
            Binary(RookLexer.GreaterThan, 8);
            Binary(RookLexer.LessThanOrEqual, 8);
            Binary(RookLexer.GreaterThanOrEqual, 8);

            Binary(RookLexer.Equal, 7);
            Binary(RookLexer.NotEqual, 7);

            Binary(RookLexer.And, 6);
            Binary(RookLexer.Or, 5);
            Binary(RookLexer.NullCoalesce, 4);
        }

        private void Atom(TokenKind kind, AtomNodeBuilder<Expression> createAtomNode)
        {
            Expression.Atom(kind, createAtomNode);
        }

        private void Unit(TokenKind kind, Parser<Expression> parselet)
        {
            Expression.Unit(kind, parselet);
        }

        private void Prefix(Operator operation, int precedence)
        {
            Expression.Prefix(operation, precedence, (symbol, operand) => new Call(symbol.Position, symbol.Literal, operand));
        }

        private void Binary(Operator operation, int precedence)
        {
            Expression.Binary(operation, precedence, (left, symbol, right) => new Call(symbol.Position, symbol.Literal, left, right));
        }

        private void Extend(Operator operation, int precedence, ExtendParserBuilder<Expression> createExtendParser)
        {
            Expression.Extend(operation, precedence, createExtendParser);
        }

        private Parser<Expression> Call(Expression callable)
        {
            return from arguments in Tuple(Expression)
                   select new Call(callable.Position, callable, arguments);

        }

        private Parser<Expression> Subscript(Expression callable)
        {
            return from arguments in Between("[", OneOrMore(Expression, Token(":")), "]")
                   select new Call(callable.Position,
                                   new Name(callable.Position, arguments.Count() == 1 ? "Index" : "Slice"),
                                   new[] { callable }.Concat(arguments));
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
    }

    public static class GrammarExtensions
    {
        /// <summary>
        /// goal.TerminatedBy(terminator) parse goal and then terminator.  If goal and terminator both
        /// succeed, the result of the goal parser is returned.
        /// </summary>
        public static Parser<T> TerminatedBy<T, S>(this Parser<T> goal, Parser<S> terminator)
        {
            return from G in goal
                   from _ in terminator
                   select G;
        }
    }
}