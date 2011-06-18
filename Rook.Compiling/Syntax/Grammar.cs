using System.Collections.Generic;
using Parsley;

namespace Rook.Compiling.Syntax
{
    public partial class Grammar : AbstractGrammar
    {
        private Grammar() {}

        private static Parser<T> Between<T>(string openOperator, Parser<T> parse, string closeOperator)
        {
            return Between(Operator(openOperator), parse, Operator(closeOperator));
        }

        private static Parser<T> Parenthesized<T>(Parser<T> parse)
        {
            return Between(Operator("("), parse, Operator(")"));
        }

        private static Parser<IEnumerable<T>> Tuple<T>(Parser<T> item)
        {
            return Parenthesized(ZeroOrMore(item, Operator(",")));
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