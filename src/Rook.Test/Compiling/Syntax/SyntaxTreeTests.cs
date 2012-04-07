using System.Linq;
using Parsley;
using Rook.Compiling.Types;
using Should;

namespace Rook.Compiling.Syntax
{
    public abstract class SyntaxTreeTests<TSyntax> where TSyntax : SyntaxTree
    {
        protected static RookGrammar RookGrammar { get { return new RookGrammar(); } }
        protected static DataType Integer { get { return NamedType.Integer; } }
        protected static DataType Boolean { get { return NamedType.Boolean; } }

        protected TSyntax Parse(string source)
        {
            var tokens = new RookLexer().Tokenize(source);
            return Parser.Parse(new TokenStream(tokens)).Value;
        }

        protected Reply<TSyntax> Parses(string source)
        {
            return Parser.Parses(source);
        }

        protected Reply<TSyntax> FailsToParse(string source)
        {
            return Parser.FailsToParse(source);
        }

        protected static void AssertTypeCheckError(TypeChecked<TSyntax> typeChecked, Position expectedPosition, string expectedMessage)
        {
            typeChecked.Syntax.ShouldBeNull();
            typeChecked.HasErrors.ShouldBeTrue();
            
            if (typeChecked.Errors.Count() != 1)
            {
                Fail.WithErrors(typeChecked.Errors, expectedPosition, expectedMessage);
            }
            else
            {
                var error = typeChecked.Errors.First();

                if (expectedPosition != error.Position || expectedMessage != error.Message)
                    Fail.WithErrors(typeChecked.Errors, expectedPosition, expectedMessage);
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