using System.Collections.Generic;
using System.Linq;
using Rook.Compiling.Syntax;
using Rook.Compiling.Types;

namespace Rook.Compiling.CodeGeneration
{
    public class CSharpTranslator : Visitor<WriteAction>
    {
        public WriteAction Visit(CompilationUnit compilationUnit)
        {
            var classes = compilationUnit.Classes.Select(Translate);
            var functions = compilationUnit.Functions.Select(Translate);

            return
                Each(
                    Using("System", "System.Collections.Generic", "Rook.Core", "Rook.Core.Collections"),
                    EndLine(),
                    Line("public class " + ReservedName.__program__ + " : Prelude"),
                    Block(classes.Concat(functions)));
        }

        private static WriteAction Using(params string[] namespaces)
        {
            return Each(namespaces.Select(ns => Line("using @;", Literal(ns))));
        }

        public WriteAction Visit(Class @class)
        {
            return Each(
                Line("public class @", Translate(@class.Name)),
                Block(@class.Methods.Select(m => Translate(m, isStatic: false))));
        }

        public WriteAction Visit(Function function)
        {
            return Translate(function, isStatic: true);
        }

        private WriteAction Translate(Function function, bool isStatic)
        {
            return Each(
                Line("public@ @ @(@)",
                     Literal(isStatic ? " static" : ""),
                     Translate(function.ReturnType),
                     Translate(function.Name),
                     Translate(function.Parameters, ", ")),
                Block(Line("return @;", Translate(function.Body))));
        }

        public WriteAction Visit(Name name)
        {
            return Literal(name.Identifier);
        }

        public WriteAction Visit(Parameter parameter)
        {
            return Format("@ @", Translate(parameter.Type), Literal(parameter.Identifier));
        }

        public WriteAction Visit(Block block)
        {
            var variableDeclarations = block.VariableDeclarations;
            var expressions = block.InnerExpressions.ToArray();

            var sideEffectExpressions = expressions.Take(expressions.Length - 1);
            var resultExpression = expressions.Last();

            return Each(
                Literal(ReservedName.__block__+"(() =>"),
                EndLine(),
                Block(
                    Each(variableDeclarations.Select(declaration => Line("", Translate(declaration)))),
                    Each(sideEffectExpressions.Select(expression => Line(ReservedName.__evaluate__+"(@);", Translate(expression)))),
                    Line("return @;", Translate(resultExpression))
                    ),
                Indentation(),
                Literal(")")
                );
        }

        public WriteAction Visit(Lambda lambda)
        {
            return Format("(@) => @", Translate(lambda.Parameters, ", "), Translate(lambda.Body));
        }

        public WriteAction Visit(If conditional)
        {
            return Format("((@) ? (@) : (@))",
                          Translate(conditional.Condition),
                          Translate(conditional.BodyWhenTrue),
                          Translate(conditional.BodyWhenFalse));
        }

        public WriteAction Visit(VariableDeclaration variableDeclaration)
        {
            return Format("@ @ = @;",
                          Translate(variableDeclaration.Type),
                          Literal(variableDeclaration.Identifier),
                          Translate(variableDeclaration.Value));
        }

        public WriteAction Visit(Call call)
        {
            var callable = call.Callable;
            var arguments = call.Arguments.ToArray();
            bool isOperator = call.IsOperator;

            if (isOperator && arguments.Length != 1)
            {
                var nameCallable = callable as Name;

                if (nameCallable != null)
                {
                    //C#'s ?? operator does not work on Rook.Core.Nullable, so
                    //we must use the ?: operator to implement the logic explicitly.
                    if (nameCallable.Identifier == "??")
                    {
                        return Format("(((@) != null) ? ((@).Value) : (@))",
                                      Translate(arguments[0]),
                                      Translate(arguments[0]),
                                      Translate(arguments[1]));
                        
                    }
                }

                return Format("((@) @ (@))",
                              Translate(arguments[0]),
                              Translate(callable),
                              Translate(arguments[1]));
            }

            return Format("(@(@))", Translate(callable), Translate(arguments, ", "));
        }

        public WriteAction Visit(New @new)
        {
            return Format("(new @())", Translate(@new.TypeName));
        }

        public WriteAction Visit(BooleanLiteral booleanLiteral)
        {
            return Literal(booleanLiteral.Value ? "true" : "false");
        }

        public WriteAction Visit(IntegerLiteral integerLiteral)
        {
            return Literal(integerLiteral.Digits);
        }

        public WriteAction Visit(StringLiteral stringLiteral)
        {
            return Literal(stringLiteral.QuotedLiteral);
        }

        public WriteAction Visit(Null nullLiteral)
        {
            return Literal("null");
        }

        public WriteAction Visit(VectorLiteral vectorLiteral)
        {
            return Format(ReservedName.__vector__ + "(@)", Translate(vectorLiteral.Items, ", "));
        }

        private static WriteAction Translate(NamedType type)
        {
            return Literal(type.ToString());
        }

        private static WriteAction Translate(DataType type)
        {
            return Literal(type.ToString());
        }

        private WriteAction Translate<T>(IEnumerable<T> items, string separator)
            where T : SyntaxTree
        {
            WriteAction sep = code => code.Literal(separator);

            WriteAction[] translatedItems = items.Select(Translate).ToArray();
            int count = translatedItems.Length;

            if (count > 0)
                return Each(
                    translatedItems
                        .Take(count - 1)
                        .Select(x => Each(x, sep))
                        .Concat(new[] {translatedItems.Last()}));

            return code => {};
        }

        private WriteAction Translate<T>(T item)
            where T : SyntaxTree
        {
            return item.Visit(this);
        }

        private static WriteAction Literal(string literal)
        {
            return code => code.Literal(literal);
        }

        private static WriteAction Line(string line)
        {
            return code => code.Line(line);
        }

        private static WriteAction Indentation()
        {
            return code => code.Indentation();
        }

        private static WriteAction EndLine()
        {
            return code => code.EndLine();
        }

        private static WriteAction Line(string format, params WriteAction[] writeActions)
        {
            return Each(Indentation(), Format(format, writeActions), EndLine());
        }

        private static WriteAction Format(string format, params WriteAction[] writeActions)
        {
            var list = new List<WriteAction>();
            string[] parts = format.Split('@');
            int i = 0;

            foreach (string part in parts)
            {
                list.Add(Literal(part));

                if (i < writeActions.Length)
                {
                    list.Add(writeActions[i]);
                    i++;
                }
            }

            return Each(list);
        }

        private static WriteAction Block(params WriteAction[] body)
        {
            return Block((IEnumerable<WriteAction>) body);
        }

        private static WriteAction Block(IEnumerable<WriteAction> body)
        {
            return Each(Line("{"), Each(body), Line("}"));
        }

        private static WriteAction Each(IEnumerable<WriteAction> writers)
        {
            return code =>
            {
                foreach (WriteAction write in writers)
                    write(code);
            };
        }

        private static WriteAction Each(params WriteAction[] writeActions)
        {
            return Each((IEnumerable<WriteAction>)writeActions);
        }
    }
}