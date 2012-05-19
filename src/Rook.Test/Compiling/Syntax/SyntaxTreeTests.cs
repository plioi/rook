using Parsley;
using Rook.Compiling.Types;

namespace Rook.Compiling.Syntax
{
    public abstract class SyntaxTreeTests<TSyntax> where TSyntax : SyntaxTree
    {
        protected static RookGrammar RookGrammar { get { return new RookGrammar(); } }
        protected static DataType Integer { get { return NamedType.Integer; } }
        protected static DataType Boolean { get { return NamedType.Boolean; } }

        protected TSyntax Parse(string source)
        {
            return Parses(source).Value;
        }

        protected Reply<TSyntax> Parses(string source)
        {
            return Parser.Parses(source);
        }

        protected Reply<TSyntax> FailsToParse(string source)
        {
            return Parser.FailsToParse(source);
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