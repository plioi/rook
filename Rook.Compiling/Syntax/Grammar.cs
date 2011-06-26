using System.Collections.Generic;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public partial class Grammar : AbstractGrammar
    {
        protected Grammar() {}

        private static Parser<T> Between<T>(string openOperator, Parser<T> parse, string closeOperator)
        {
            return Between(Token(openOperator), parse, Token(closeOperator));
        }

        private static Parser<T> Parenthesized<T>(Parser<T> parse)
        {
            return Between("(", parse, ")");
        }

        private static Parser<IEnumerable<T>> Tuple<T>(Parser<T> item)
        {
            return Parenthesized(ZeroOrMore(item, Token(",")));
        }

        public static Parser<Name> Name
        {
            get
            {
                return from identifier in Identifier
                       select new Name(identifier.Position, identifier.Literal);
            }
        }
    }
}