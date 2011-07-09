using System.Linq;
using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public abstract class SyntaxTreeSpec<TSyntax> where TSyntax : SyntaxTree
    {
        protected static readonly DataType Integer = NamedType.Integer;
        protected static readonly DataType Boolean = NamedType.Boolean;

        protected TSyntax Parse(string source)
        {
            return Parser(new RookLexer(source)).Value;
        }

        protected Reply<TSyntax> Parses(string source)
        {
            return Parser.Parses(source);
        }

        protected Reply<TSyntax> FailsToParse(string source, string expectedUnparsedSource)
        {
            return Parser.FailsToParse(source, expectedUnparsedSource);
        }

        protected static void AssertTypeCheckError(TypeChecked<TSyntax> typeChecked, int line, int column, string expectedMessage)
        {
            typeChecked.Syntax.ShouldBeNull("Expected type check error but found type checked syntax.");
            typeChecked.HasErrors.ShouldBeTrue();
            
            if (typeChecked.Errors.Count() != 1)
            {
                Fail.WithErrors(typeChecked.Errors, line, column, expectedMessage);
            }
            else
            {
                CompilerError error = typeChecked.Errors.First();

                if (line != error.Line || column != error.Column || expectedMessage != error.Message)
                    Fail.WithErrors(typeChecked.Errors, line, column, expectedMessage);
            }
        }

        protected abstract Parser<TSyntax> Parser { get; }

        protected delegate DataType TypeMapping(string name);

        protected static Environment Environment(params TypeMapping[] symbols)
        {
            var rootEnvironment = new Environment();
            var environment = new Environment(rootEnvironment);

            foreach (var symbol in symbols)
            {
                var item = symbol(null);
                var name = symbol.Method.GetParameters()[0].Name;
                environment[name] = item;
            }

            return environment;
        }
    }
}